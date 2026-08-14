import fs from "node:fs/promises";
import path from "node:path";
import { SpreadsheetFile, Workbook } from "@oai/artifact-tool";

if (process.argv.length < 3) {
  console.error("Usage: node build_study_workbooks.mjs <workbook_spec.json>");
  process.exit(1);
}

const specPath = process.argv[2];
const spec = JSON.parse(await fs.readFile(specPath, "utf8"));

function toCellValue(value) {
  if (value === undefined || value === null || value === "") {
    return null;
  }

  if (typeof value === "number" || typeof value === "boolean") {
    return value;
  }

  return String(value);
}

function padRows(rows) {
  const width = rows.reduce((max, row) => Math.max(max, row.length), 0);
  return rows.map((row) => {
    const padded = [...row];
    while (padded.length < width) {
      padded.push(null);
    }
    return padded.map(toCellValue);
  });
}

async function buildWorkbook(workbookSpec) {
  const workbook = Workbook.create();
  const workbookDir = path.dirname(workbookSpec.output_path);
  const workbookBase = path.basename(workbookSpec.output_path, path.extname(workbookSpec.output_path));

  for (const [sheetIndex, sheetSpec] of workbookSpec.sheets.entries()) {
    const sheet = workbook.worksheets.add(sheetSpec.name);
    sheet.showGridLines = false;

    const rows = padRows(sheetSpec.rows);
    if (rows.length === 0) {
      continue;
    }

    const rowCount = rows.length;
    const colCount = rows[0].length;
    const usedRange = sheet.getRangeByIndexes(0, 0, rowCount, colCount);
    usedRange.values = rows;

    const headerRange = sheet.getRangeByIndexes(0, 0, 1, colCount);
    headerRange.format = {
      fill: "#1F4E5F",
      font: { bold: true, color: "#FFFFFF" },
      wrapText: true,
      horizontalAlignment: "Center",
      verticalAlignment: "Center",
      borders: { preset: "all", style: "thin", color: "#D9D9D9" },
    };

    if (rowCount > 1) {
      const bodyRange = sheet.getRangeByIndexes(1, 0, rowCount - 1, colCount);
      bodyRange.format = {
        wrapText: !!sheetSpec.wrap_text,
        verticalAlignment: "Top",
        borders: { preset: "all", style: "thin", color: "#E6E6E6" },
      };
    }

    sheet.freezePanes.freezeRows(1);

    if (Array.isArray(sheetSpec.column_widths)) {
      sheetSpec.column_widths.forEach((width, colIndex) => {
        sheet.getRangeByIndexes(0, colIndex, rowCount, 1).format.columnWidth = width;
      });
    } else {
      usedRange.format.autofitColumns();
    }

    usedRange.format.autofitRows();

    const preview = await workbook.render({
      sheetName: sheetSpec.name,
      autoCrop: "all",
      scale: 1,
      format: "png",
    });

    const previewPath = path.join(workbookDir, "..", "tmp", `${workbookBase}_${sheetIndex + 1}_${sheetSpec.name}.png`);
    await fs.mkdir(path.dirname(previewPath), { recursive: true });
    await fs.writeFile(previewPath, new Uint8Array(await preview.arrayBuffer()));
  }

  await fs.mkdir(path.dirname(workbookSpec.output_path), { recursive: true });
  const output = await SpreadsheetFile.exportXlsx(workbook);
  await output.save(workbookSpec.output_path);
}

for (const workbookSpec of spec.workbooks) {
  await buildWorkbook(workbookSpec);
}

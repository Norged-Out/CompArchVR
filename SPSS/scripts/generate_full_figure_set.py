#!/usr/bin/env python3
"""
Generate the approved dissertation quantitative figure set from the cleaned
SPSS quantitative CSV.

Expected input:
    SPSS_quantitative.csv

Approved figures:
    01 Usability Likert
    02 Engagement Likert
    03 Learning Experience Likert
    04 Discomfort distribution
    05 Post-session knowledge performance (Q11-Q25)
    06 Baseline vs post-session repeated knowledge (Q11-Q14, Q25)

Outputs:
    PNG (300 dpi)

Design rules:
    - no title inside figures; LaTeX captions handle figure titles/explanation
    - full-width LaTeX friendly
    - consistent typography and spacing
    - participant count inferred automatically from the input CSV
    - percentage/count labels embedded where useful
    - fail loudly if required variables are missing or malformed
"""

from __future__ import annotations

import argparse
from pathlib import Path

import matplotlib
matplotlib.use("Agg")
import matplotlib.pyplot as plt
import numpy as np
import pandas as pd


LIKERT_VALUES = [1, 2, 3, 4, 5]
LIKERT_LABELS = {
    1: "Strongly Disagree",
    2: "Disagree",
    3: "Neutral",
    4: "Agree",
    5: "Strongly Agree",
}

LIKERT_FIGURES = [
    {
        "number": "01",
        "slug": "Usability_Likert",
        "items": ["USE01", "USE02", "USE03", "USE04", "USE05", "USE06"],
        "labels": [
            "Easy to learn",
            "Navigate without difficulty",
            "Controls felt intuitive",
            "Visual presentation made concepts easier to follow",
            "Could focus without interface distraction",
            "Would feel confident using this system again",
        ],
        "figsize": (11, 5.6),
    },
    {
        "number": "02",
        "slug": "Engagement_Likert",
        "items": ["ENG01", "ENG02", "ENG03", "ENG04", "ENG05"],
        "labels": [
            "Experience held my attention",
            "Felt actively involved in the learning process",
            "Made the topic feel more interesting",
            "Motivated to explore the concepts further",
            "Interactive format improved focus\ncompared with standard learning materials",
        ],
        "figsize": (11, 5.2),
    },
    {
        "number": "03",
        "slug": "Learning_Experience_Likert",
        "items": ["LEARN01", "LEARN02", "LEARN03", "LEARN04", "LEARN05"],
        "labels": [
            "VR experience helped me understand the topic better",
            "Visualisations helped me build a mental model",
            "Made abstract processes easier to understand",
            "Would prefer this as a supplement\nto traditional teaching materials",
            "Believe I learned something useful from this session",
        ],
        "figsize": (11, 5.2),
    },
]

KNOWLEDGE_LABELS = {
    11: "Q11 — Program counter",
    12: "Q12 — Instruction fetch",
    13: "Q13 — Instruction register",
    14: "Q14 — Program-counter update",
    15: "Q15 — Arithmetic stage",
    16: "Q16 — Branch / program counter",
    17: "Q17 — `add` instruction",
    18: "Q18 — `addi` instruction",
    19: "Q19 — Load instruction type",
    20: "Q20 — `$zero` register",
    21: "Q21 — `lw` instruction",
    22: "Q22 — `sw` instruction",
    23: "Q23 — `beq` instruction",
    24: "Q24 — `j` instruction",
    25: "Q25 — Instruction-stage order",
}

REPEATED_QUESTIONS = [11, 12, 13, 14, 25]


def validate_columns(df: pd.DataFrame, columns: list[str]) -> None:
    missing = [column for column in columns if column not in df.columns]
    if missing:
        raise ValueError(f"Missing expected columns: {', '.join(missing)}")


def validate_likert(df: pd.DataFrame, columns: list[str]) -> None:
    validate_columns(df, columns)
    for column in columns:
        values = df[column].dropna()
        invalid = values[~values.isin(LIKERT_VALUES)]
        if not invalid.empty:
            raise ValueError(
                f"{column} contains values outside 1-5: "
                f"{sorted(invalid.unique().tolist())}"
            )


def validate_binary(df: pd.DataFrame, columns: list[str]) -> None:
    validate_columns(df, columns)
    for column in columns:
        values = df[column].dropna()
        invalid = values[~values.isin([0, 1])]
        if not invalid.empty:
            raise ValueError(
                f"{column} contains values outside 0/1: "
                f"{sorted(invalid.unique().tolist())}"
            )


def save_figure(fig: plt.Figure, output_base: Path) -> None:
    fig.savefig(output_base.with_suffix(".png"), dpi=300, bbox_inches="tight")
    plt.close(fig)


def percentages_for_items(df: pd.DataFrame, items: list[str]) -> np.ndarray:
    percentages = []
    for item in items:
        values = df[item].dropna()
        if values.empty:
            raise ValueError(f"{item} has no valid responses.")
        counts = values.value_counts().reindex(LIKERT_VALUES, fill_value=0)
        percentages.append((counts / counts.sum() * 100).values)
    return np.asarray(percentages)


def create_likert_chart(
    df: pd.DataFrame,
    items: list[str],
    labels: list[str],
    figsize: tuple[float, float],
    output_base: Path,
    percentage_label_threshold: float = 14.0,
) -> None:
    validate_likert(df, items)

    percentages = percentages_for_items(df, items)

    # Reverse so the first questionnaire item appears at the top.
    percentages = percentages[::-1]
    labels = labels[::-1]

    sd, d, neutral, agree, sa = [percentages[:, i] for i in range(5)]

    left_sd = -(sd + d + neutral / 2)
    left_d = -(d + neutral / 2)
    left_neutral = -(neutral / 2)
    left_agree = neutral / 2
    left_sa = neutral / 2 + agree

    fig, ax = plt.subplots(figsize=figsize)
    colors = plt.rcParams["axes.prop_cycle"].by_key()["color"][:5]

    y = np.arange(len(labels))
    bar_height = 0.46

    series = [
        (sd, left_sd, LIKERT_LABELS[1], colors[0]),
        (d, left_d, LIKERT_LABELS[2], colors[1]),
        (neutral, left_neutral, LIKERT_LABELS[3], colors[2]),
        (agree, left_agree, LIKERT_LABELS[4], colors[3]),
        (sa, left_sa, LIKERT_LABELS[5], colors[4]),
    ]

    for widths, lefts, legend_label, color in series:
        ax.barh(
            y,
            widths,
            left=lefts,
            height=bar_height,
            label=legend_label,
            color=color,
        )

    for widths, lefts, _, _ in series:
        for yi, width, left in zip(y, widths, lefts):
            if width >= percentage_label_threshold:
                ax.text(
                    left + width / 2,
                    yi,
                    f"{width:.0f}%",
                    va="center",
                    ha="center",
                    fontsize=9,
                )

    ax.set_yticks(y)
    ax.set_yticklabels(labels, fontsize=10)
    ax.set_xlim(-100, 100)
    ax.set_xticks([-100, -50, 0, 50, 100])
    ax.set_xticklabels(["100%", "50%", "0%", "50%", "100%"], fontsize=9)
    ax.set_xlabel("Percentage of participants", fontsize=10)

    ax.axvline(0, linewidth=0.9)
    ax.grid(axis="x", linewidth=0.5)
    ax.set_axisbelow(True)

    ax.legend(
        loc="upper center",
        bbox_to_anchor=(0.5, -0.14),
        ncol=5,
        frameon=False,
        fontsize=9,
        handlelength=1.6,
        columnspacing=1.4,
    )

    fig.tight_layout()
    save_figure(fig, output_base)


def create_discomfort_chart(
    df: pd.DataFrame,
    output_base: Path,
) -> None:
    column = "USE07_Discomfort"
    validate_likert(df, [column])

    values = df[column].dropna()
    counts = values.value_counts().reindex(LIKERT_VALUES, fill_value=0)
    percentages = counts / counts.sum() * 100

    labels = [
        "Strongly\nDisagree",
        "Disagree",
        "Neutral",
        "Agree",
        "Strongly\nAgree",
    ]

    fig, ax = plt.subplots(figsize=(8.5, 4.8))
    bars = ax.bar(labels, percentages.values, width=0.62)

    for bar, pct, count in zip(bars, percentages.values, counts.values):
        ax.text(
            bar.get_x() + bar.get_width() / 2,
            bar.get_height() + 2,
            f"{pct:.0f}%\n(n={int(count)})",
            ha="center",
            va="bottom",
            fontsize=9,
        )

    ax.set_ylabel("Percentage of participants", fontsize=10)
    ax.set_ylim(0, max(100, float(percentages.max()) + 15))
    ax.grid(axis="y", linewidth=0.5)
    ax.set_axisbelow(True)

    fig.tight_layout()
    save_figure(fig, output_base)


def create_post_knowledge_chart(
    df: pd.DataFrame,
    output_base: Path,
) -> None:
    columns = [f"POST_Q{q}_CORR" for q in range(11, 26)]
    validate_binary(df, columns)

    rows = []
    for q in range(11, 26):
        s = df[f"POST_Q{q}_CORR"].dropna()
        if s.empty:
            raise ValueError(f"POST_Q{q}_CORR has no valid responses.")

        correct = int((s == 1).sum())
        total = int(len(s))
        pct = 100 * correct / total

        rows.append(
            {
                "question": q,
                "label": KNOWLEDGE_LABELS[q],
                "correct": correct,
                "total": total,
                "percent": pct,
            }
        )

    plot_df = pd.DataFrame(rows).iloc[::-1].reset_index(drop=True)

    fig, ax = plt.subplots(figsize=(10.8, 7.0))
    bars = ax.barh(
        plot_df["label"],
        plot_df["percent"],
        height=0.46,
    )

    # White text inside the bar: approved v3 design.
    for bar, pct, correct, total in zip(
        bars,
        plot_df["percent"],
        plot_df["correct"],
        plot_df["total"],
    ):
        ax.text(
            pct - 1.4,
            bar.get_y() + bar.get_height() / 2,
            f"{pct:.0f}% ({correct}/{total})",
            va="center",
            ha="right",
            fontsize=8.7,
            color="white",
        )

    ax.set_xlim(0, 100)
    ax.set_xticks([0, 20, 40, 60, 80, 100])
    ax.set_xticklabels(["0%", "20%", "40%", "60%", "80%", "100%"], fontsize=9)
    ax.set_xlabel("Participants answering correctly", fontsize=10)
    ax.tick_params(axis="y", labelsize=9.5)
    ax.grid(axis="x", linewidth=0.5)
    ax.set_axisbelow(True)

    fig.tight_layout()
    save_figure(fig, output_base)


def create_baseline_vs_post_chart(
    df: pd.DataFrame,
    output_base: Path,
) -> None:
    pre_columns = [f"PRE_Q{q}_CORR" for q in REPEATED_QUESTIONS]
    post_columns = [f"POST_Q{q}_CORR" for q in REPEATED_QUESTIONS]

    validate_binary(df, pre_columns + post_columns)

    rows = []
    for q in REPEATED_QUESTIONS:
        pre = df[f"PRE_Q{q}_CORR"]
        post = df[f"POST_Q{q}_CORR"]
        mask = pre.notna() & post.notna()

        n = int(mask.sum())
        if n == 0:
            raise ValueError(f"Q{q} has no matched baseline/post cases.")

        pre_correct = int((pre[mask] == 1).sum())
        post_correct = int((post[mask] == 1).sum())

        rows.append(
            {
                "question": q,
                "label": f"{KNOWLEDGE_LABELS[q]}  (n={n})",
                "n": n,
                "pre_pct": 100 * pre_correct / n,
                "post_pct": 100 * post_correct / n,
                "pre_correct": pre_correct,
                "post_correct": post_correct,
            }
        )

    plot_df = pd.DataFrame(rows).iloc[::-1].reset_index(drop=True)

    y = np.arange(len(plot_df))
    bar_height = 0.34

    fig, ax = plt.subplots(figsize=(10.8, 5.6))

    bars_pre = ax.barh(
        y - bar_height / 2,
        plot_df["pre_pct"],
        height=bar_height,
        label="Baseline",
    )
    bars_post = ax.barh(
        y + bar_height / 2,
        plot_df["post_pct"],
        height=bar_height,
        label="Post-session",
    )

    for bars, pcts, corrects in [
        (bars_pre, plot_df["pre_pct"], plot_df["pre_correct"]),
        (bars_post, plot_df["post_pct"], plot_df["post_correct"]),
    ]:
        for bar, pct, correct, total in zip(
            bars,
            pcts,
            corrects,
            plot_df["n"],
        ):
            ax.text(
                pct - 1.4,
                bar.get_y() + bar.get_height() / 2,
                f"{pct:.0f}% ({correct}/{total})",
                va="center",
                ha="right",
                fontsize=8.8,
                color="white",
            )

    ax.set_yticks(y)
    ax.set_yticklabels(plot_df["label"], fontsize=9.5)
    ax.set_xlim(0, 100)
    ax.set_xticks([0, 20, 40, 60, 80, 100])
    ax.set_xticklabels(["0%", "20%", "40%", "60%", "80%", "100%"], fontsize=9)
    ax.set_xlabel("Participants answering correctly", fontsize=10)
    ax.grid(axis="x", linewidth=0.5)
    ax.set_axisbelow(True)

    ax.legend(
        loc="upper center",
        bbox_to_anchor=(0.5, -0.13),
        ncol=2,
        frameon=False,
        fontsize=9,
    )

    fig.tight_layout()
    save_figure(fig, output_base)


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument(
        "--input",
        required=True,
        help="Path to the generated SPSS_quantitative.csv",
    )
    parser.add_argument(
        "--output-dir",
        required=True,
        help="Directory for generated figure files",
    )
    args = parser.parse_args()

    input_path = Path(args.input)
    output_dir = Path(args.output_dir)
    output_dir.mkdir(parents=True, exist_ok=True)

    df = pd.read_csv(input_path)
    participant_count = len(df)

    if participant_count == 0:
        raise ValueError("The quantitative CSV contains zero participants.")

    # Figures 01-03: approved diverging Likert charts.
    for config in LIKERT_FIGURES:
        output_base = output_dir / (
            f"Figure_{config['number']}_{config['slug']}_{participant_count}P"
        )
        create_likert_chart(
            df=df,
            items=config["items"],
            labels=config["labels"],
            figsize=config["figsize"],
            output_base=output_base,
        )
        print(f"Generated: {output_base.name}.png")

    # Figure 04: discomfort distribution.
    output_base = output_dir / f"Figure_04_Discomfort_{participant_count}P"
    create_discomfort_chart(df, output_base)
    print(f"Generated: {output_base.name}.png")

    # Figure 05: Q11-Q25 post-session percent correct.
    output_base = (
        output_dir
        / f"Figure_05_Post_Knowledge_Percent_Correct_{participant_count}P"
    )
    create_post_knowledge_chart(df, output_base)
    print(f"Generated: {output_base.name}.png")

    # Figure 06: matched item-level baseline vs post-session comparison.
    output_base = (
        output_dir
        / f"Figure_06_Baseline_vs_Post_Repeated_Knowledge_{participant_count}P"
    )
    create_baseline_vs_post_chart(df, output_base)
    print(f"Generated: {output_base.name}.png")


if __name__ == "__main__":
    main()

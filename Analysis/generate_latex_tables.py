"""Generate LaTeX tables from significance result CSV files."""

from pathlib import Path
import pandas as pd

output_dir = Path("Analysis/output")

main_table = pd.read_csv(output_dir / "results_main_table.csv")
full_table = pd.read_csv(output_dir / "results_appendix_full_table.csv")


def format_p_value(p_val_str):
    """Convert p-value string to a thesis-friendly format."""
    if pd.isna(p_val_str) or p_val_str == "N/A":
        return "N/A"
    try:
        p_float = float(p_val_str)
        if p_float < 0.001:
            return "< 0.001"
        return f"{p_float:.4f}"
    except Exception:
        return str(p_val_str)


def format_row(row):
    metric = str(row["Metric"]).replace("_", "\\_")
    h_stat = f"{row['H-statistic']:.4f}" if pd.notna(row["H-statistic"]) else "N/A"
    p_val = format_p_value(row["p-value (formatted)"])
    sig = str(row["Significance"])
    no_bears = f"{row['No bear mean']:.1f}" if pd.notna(row["No bear mean"]) else "N/A"
    avg_bears = f"{row['Average bear mean']:.1f}" if pd.notna(row["Average bear mean"]) else "N/A"
    many_bears = f"{row['many bear mean']:.1f}" if pd.notna(row["many bear mean"]) else "N/A"
    return f"{metric} & {h_stat} & {p_val} & {sig} & {no_bears} & {avg_bears} & {many_bears} \\\\"


def build_standard_table(df, caption, label):
    latex = (
        "\\begin{table}[h!]\n"
        "\\centering\n"
        f"\\caption{{{caption}}}\n"
        f"\\label{{{label}}}\n"
        "\\small\n"
        "\\begin{tabular}{lcccccc}\n"
        "\\toprule\n"
        "\\textbf{Metric} & \\textbf{H-statistic} & \\textbf{p-value} & \\textbf{Sig} & "
        "\\textbf{No Bears} & \\textbf{Avg Bears} & \\textbf{Many Bears} \\\\n"
        "\\midrule\n"
    )
    for _, row in df.iterrows():
        latex += format_row(row) + "\n"
    latex += "\\bottomrule\n\\end{tabular}\n\\end{table}\n"
    return latex


def build_longtable(df):
    latex = (
        "\\begin{longtable}[]{@{}lcccccc@{}}\n"
        "\\caption{Complete Kruskal-Wallis H-test results for all metrics.}"
        "\\label{tab:significance_full} \\\\n"
        "\\toprule\n"
        "\\textbf{Metric} & \\textbf{H-stat} & \\textbf{p-value} & \\textbf{Sig} & "
        "\\textbf{No Bears} & \\textbf{Avg Bears} & \\textbf{Many Bears} \\\\n"
        "\\midrule\n"
        "\\endfirsthead\n"
        "\\multicolumn{7}{@{}l}{\\textit{Table \\ref{tab:significance_full} continued}} \\\\n"
        "\\toprule\n"
        "\\textbf{Metric} & \\textbf{H-stat} & \\textbf{p-value} & \\textbf{Sig} & "
        "\\textbf{No Bears} & \\textbf{Avg Bears} & \\textbf{Many Bears} \\\\n"
        "\\midrule\n"
        "\\endhead\n"
        "\\midrule\n"
        "\\multicolumn{7}{r}{\\textit{Continued on next page}} \\\\n"
        "\\endfoot\n"
        "\\bottomrule\n"
        "\\endlastfoot\n"
    )
    for _, row in df.iterrows():
        latex += format_row(row) + "\n"
    latex += "\\end{longtable}"
    return latex


# Existing 15-most-significant table (kept for compatibility)
latex_main_15 = build_standard_table(
    main_table,
    "Kruskal-Wallis H-test results for the 15 most significant metrics. "
    "Statistical significance is denoted by asterisks: *** p < 0.001, ** p < 0.01, * p < 0.05.",
    "tab:significance_main",
)

# New: top 10 overall for results section
top10_overall = full_table.head(10).copy()
latex_top10_overall = build_standard_table(
    top10_overall,
    "Top 10 most significant metrics (overall) from Kruskal-Wallis H-test.",
    "tab:significance_top10_overall",
)

# New: top 10 non-bear metrics for results section
top10_non_bear = full_table[~full_table["Metric"].str.startswith("Bear", na=False)].head(10).copy()
latex_top10_non_bear = build_standard_table(
    top10_non_bear,
    "Top 10 most significant non-bear metrics from Kruskal-Wallis H-test.",
    "tab:significance_top10_nonbear",
)

latex_full = build_longtable(full_table)

with open(output_dir / "results_main_table.tex", "w", encoding="utf-8") as f:
    f.write(latex_main_15)

with open(output_dir / "results_top10_overall.tex", "w", encoding="utf-8") as f:
    f.write(latex_top10_overall)

with open(output_dir / "results_top10_non_bear.tex", "w", encoding="utf-8") as f:
    f.write(latex_top10_non_bear)

with open(output_dir / "results_appendix_full_table.tex", "w", encoding="utf-8") as f:
    f.write(latex_full)

print("✓ LaTeX tables generated:")
print(f"  1. {output_dir / 'results_main_table.tex'}")
print(f"  2. {output_dir / 'results_top10_overall.tex'}")
print(f"  3. {output_dir / 'results_top10_non_bear.tex'}")
print(f"  4. {output_dir / 'results_appendix_full_table.tex'}")

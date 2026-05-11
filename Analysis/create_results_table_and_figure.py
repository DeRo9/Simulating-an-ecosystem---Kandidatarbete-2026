

import pandas as pd
import matplotlib.pyplot as plt
import numpy as np
from pathlib import Path

# Paths
output_dir = Path("Analysis/output")
excel_file = output_dir / "results_table.xlsx"

# Load data from Excel
print(f"Reading Excel file: {excel_file}")
excel_df = pd.read_excel(excel_file)

print(f"Columns in Excel: {excel_df.columns.tolist()}")
print(excel_df.head())

# Rename columns to match our expected names
excel_df.rename(columns={
    'metric': 'Metric',
    'p_value': 'p-value',
    'H_statistic': 'H-statistic',
    'significant (p<0,05)': 'Significant'
}, inplace=True)


# Sort by p-value
sig_df_sorted = excel_df.sort_values("p-value")

# Filter only significant ones
sig_df_sig = sig_df_sorted[sig_df_sorted["p-value"] < 0.05].head(15)

# Add significance markers
def add_sig_marker(p):
    if pd.isna(p):
        return "N/A"
    if p < 0.001:
        return "***"
    elif p < 0.01:
        return "**"
    elif p < 0.05:
        return "*"
    else:
        return "ns"

sig_df_sig_copy = sig_df_sig.copy()
sig_df_sig_copy["Significance"] = sig_df_sig_copy["p-value"].apply(add_sig_marker)

# Format p-value for display
sig_df_sig_copy["p-value (formatted)"] = sig_df_sig_copy["p-value"].apply(lambda x: f"{x:.2e}" if pd.notna(x) else "N/A")

# Create main table with selected columns
main_table_cols = ["Metric", "H-statistic", "p-value (formatted)", "Significance"]
if "No bear mean" in sig_df_sig_copy.columns:
    main_table_cols.extend(["No bear mean", "Average bear mean", "many bear mean"])

main_table = sig_df_sig_copy[main_table_cols].copy()
main_table.to_csv(output_dir / "results_main_table.csv", index=False)

print(f"\n✓ Main table saved: {output_dir / 'results_main_table.csv'}")
print("\nMain Table (Top 15 Significant Metrics):")
print(main_table.to_string())



fig, ax = plt.subplots(figsize=(14, 10))

# Prepare data - use all rows from Excel
metrics = sig_df_sorted["Metric"].values
p_values = sig_df_sorted["p-value"].values

# Handle NaN p-values (for metrics without valid tests)
valid_idx = pd.notna(p_values)
metrics_valid = metrics[valid_idx]
p_values_valid = p_values[valid_idx]

# Color code by significance
colors = ["darkred" if p < 0.001 else "red" if p < 0.01 else "orange" if p < 0.05 else "lightgray" 
          for p in p_values_valid]

# Bar plot with -log10 transformation for better visualization
y_pos = np.arange(len(metrics_valid))
log_p = -np.log10(p_values_valid)

ax.barh(y_pos, log_p, color=colors, edgecolor="black", linewidth=0.5)

# Add significance threshold line at α = 0.05 (which is -log10(0.05) ≈ 1.3)
ax.axvline(-np.log10(0.05), color="blue", linestyle="--", linewidth=2, label="α = 0.05")

ax.set_yticks(y_pos)
ax.set_yticklabels(metrics_valid, fontsize=8)
ax.set_xlabel("-log10(p-value)", fontsize=12, fontweight="bold")
ax.set_title("Significance Test Results (Kruskal-Wallis H-test)", fontsize=14, fontweight="bold")
ax.invert_yaxis()
ax.grid(axis="x", alpha=0.3, linestyle=":")

# Legend
from matplotlib.patches import Patch
legend_elements = [
    Patch(facecolor="darkred", edgecolor="black", label="p < 0.001 (***)")
    , Patch(facecolor="red", edgecolor="black", label="p < 0.01 (**)")
    , Patch(facecolor="orange", edgecolor="black", label="p < 0.05 (*)")
    , Patch(facecolor="lightgray", edgecolor="black", label="p ≥ 0.05 (ns)")
]
ax.legend(handles=legend_elements, loc="lower right", fontsize=10)

plt.tight_layout()
plt.savefig(output_dir / "significance_figure.png", dpi=300, bbox_inches="tight")
print(f"\n✓ Significance figure saved: {output_dir / 'significance_figure.png'}")

appendix_table = sig_df_sorted.copy()
appendix_table["p-value (formatted)"] = appendix_table["p-value"].apply(
    lambda x: f"{x:.2e}" if pd.notna(x) else "N/A"
)
appendix_table["Significance"] = appendix_table["p-value"].apply(add_sig_marker)

# Select columns for appendix
appendix_cols = ["Metric", "H-statistic", "p-value (formatted)", "Significance"]
if "No bear mean" in appendix_table.columns:
    appendix_cols.extend(["No bear mean", "Average bear mean", "many bear mean"])

appendix_output = appendix_table[appendix_cols]
appendix_output.to_csv(output_dir / "results_appendix_full_table.csv", index=False)

print(f"✓ Full appendix table saved: {output_dir / 'results_appendix_full_table.csv'}")


n_total = len(sig_df_sorted)
n_significant = (sig_df_sorted["p-value"] < 0.05).sum()
n_very_sig = (sig_df_sorted["p-value"] < 0.001).sum()

print(f"\n{'='*50}")
print(f"SUMMARY STATISTICS")
print(f"{'='*50}")
print(f"Total metrics tested: {n_total}")
print(f"Significant (p < 0.05): {n_significant} ({100*n_significant/n_total:.1f}%)")
print(f"Very significant (p < 0.001): {n_very_sig} ({100*n_very_sig/n_total:.1f}%)")
print(f"{'='*50}\n")

print("✓ All outputs generated successfully!")
print(f"\nFiles created:")
print(f"  1. {output_dir / 'results_main_table.csv'}")
print(f"  2. {output_dir / 'significance_figure.png'}")
print(f"  3. {output_dir / 'results_appendix_full_table.csv'}")

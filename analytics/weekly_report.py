from __future__ import annotations

import csv
import sys
from collections import Counter, defaultdict
from datetime import datetime
from pathlib import Path


def load_rows(csv_path: Path) -> list[dict[str, str]]:
    with csv_path.open(newline="", encoding="utf-8") as handle:
        return list(csv.DictReader(handle))


def summarize(rows: list[dict[str, str]]) -> tuple[list[dict[str, str]], list[tuple[str, int]]]:
    by_day: dict[str, int] = defaultdict(int)
    statuses: Counter[str] = Counter()

    for row in rows:
        reservation_date = datetime.fromisoformat(row["ReservationDate"])
        by_day[reservation_date.strftime("%A")] += int(row["PartySize"])
        statuses[row["Status"]] += 1

    summary_rows = [
        {"Metric": "Total Reservations", "Value": str(len(rows))},
        {"Metric": "Confirmed Reservations", "Value": str(statuses.get("Confirmed", 0))},
        {"Metric": "Pending Reservations", "Value": str(statuses.get("Pending", 0))},
        {"Metric": "Canceled Reservations", "Value": str(statuses.get("Canceled", 0))},
    ]

    busiest = sorted(by_day.items(), key=lambda item: item[1], reverse=True)
    return summary_rows, busiest


def write_summary(output_path: Path, summary_rows: list[dict[str, str]], busiest: list[tuple[str, int]]) -> None:
    with output_path.open("w", newline="", encoding="utf-8") as handle:
        writer = csv.DictWriter(handle, fieldnames=["Metric", "Value"])
        writer.writeheader()
        writer.writerows(summary_rows)
        writer.writerow({"Metric": "Predicted Busy Day", "Value": busiest[0][0] if busiest else "No data"})


def main() -> int:
    if len(sys.argv) < 3:
        print("Usage: python weekly_report.py input.csv output.csv")
        return 1

    input_path = Path(sys.argv[1])
    output_path = Path(sys.argv[2])

    rows = load_rows(input_path)
    summary_rows, busiest = summarize(rows)
    write_summary(output_path, summary_rows, busiest)

    print(f"Wrote summary to {output_path}")
    if busiest:
        print(f"Busiest day: {busiest[0][0]} ({busiest[0][1]} guests)")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())

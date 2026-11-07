from pathlib import Path
import sys

from docx import Document


def iter_block_text(doc):
    for i, paragraph in enumerate(doc.paragraphs):
        text = paragraph.text.strip()
        if text:
            print(f"P{i}: {text}")

    for ti, table in enumerate(doc.tables):
        print(f"\nTABLE {ti} ({len(table.rows)}x{len(table.columns)})")
        for ri, row in enumerate(table.rows):
            cells = [cell.text.replace("\n", " / ").strip() for cell in row.cells]
            print(f"R{ri}: " + " | ".join(cells))


for arg in sys.argv[1:]:
    path = Path(arg)
    print(f"\n===== {path} =====")
    iter_block_text(Document(path))

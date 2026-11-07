from pathlib import Path
from zipfile import ZipFile

from docx import Document


folder = Path(r"D:\UnityProjects\_plus\artifacts\实验报告")
required = ["贾宇轩", "202431061704", "油花花食堂订餐系统"]

for path in sorted(folder.glob("*.docx")):
    doc = Document(path)
    text = "\n".join(p.text for p in doc.paragraphs)
    text += "\n" + "\n".join(cell.text for table in doc.tables for row in table.rows for cell in row.cells)
    with ZipFile(path) as archive:
        images = [n for n in archive.namelist() if n.startswith("word/media/")]
    missing = [item for item in required if item not in text]
    print(f"{path.name}\tparagraphs={len(doc.paragraphs)}\ttables={len(doc.tables)}\timages={len(images)}\tmissing={missing}")

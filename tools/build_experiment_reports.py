from pathlib import Path

from docx import Document
from docx.enum.section import WD_SECTION
from docx.enum.table import WD_CELL_VERTICAL_ALIGNMENT, WD_TABLE_ALIGNMENT
from docx.enum.text import WD_ALIGN_PARAGRAPH
from docx.oxml import OxmlElement
from docx.oxml.ns import qn
from docx.shared import Cm, Pt, RGBColor


ROOT = Path(r"D:\UnityProjects\_plus")
SHOT_DIR = ROOT / "artifacts" / "实验截图"
OUT_DIR = ROOT / "artifacts" / "实验报告"
OUT_DIR.mkdir(parents=True, exist_ok=True)

NAME = "贾宇轩"
STUDENT_ID = "202431061704"
PROJECT = "油花花食堂订餐系统"


def shade_cell(cell, fill):
    tc_pr = cell._tc.get_or_add_tcPr()
    shd = OxmlElement("w:shd")
    shd.set(qn("w:fill"), fill)
    tc_pr.append(shd)


def set_cell_text(cell, text, bold=False, color=None):
    cell.text = ""
    p = cell.paragraphs[0]
    p.alignment = WD_ALIGN_PARAGRAPH.CENTER
    r = p.add_run(str(text))
    r.bold = bold
    r.font.name = "微软雅黑"
    r._element.rPr.rFonts.set(qn("w:eastAsia"), "微软雅黑")
    r.font.size = Pt(10.5)
    if color:
        r.font.color.rgb = RGBColor(*color)
    cell.vertical_alignment = WD_CELL_VERTICAL_ALIGNMENT.CENTER


def setup_document(title, subtitle):
    doc = Document()
    sec = doc.sections[0]
    sec.top_margin = Cm(2.2)
    sec.bottom_margin = Cm(2.0)
    sec.left_margin = Cm(2.2)
    sec.right_margin = Cm(2.0)

    styles = doc.styles
    normal = styles["Normal"]
    normal.font.name = "宋体"
    normal._element.rPr.rFonts.set(qn("w:eastAsia"), "宋体")
    normal.font.size = Pt(11)
    normal.paragraph_format.line_spacing = 1.5
    normal.paragraph_format.space_after = Pt(5)

    for style_name, size, color in [
        ("Title", 24, (31, 78, 121)),
        ("Heading 1", 16, (31, 78, 121)),
        ("Heading 2", 13, (47, 84, 150)),
    ]:
        style = styles[style_name]
        style.font.name = "微软雅黑"
        style._element.rPr.rFonts.set(qn("w:eastAsia"), "微软雅黑")
        style.font.size = Pt(size)
        style.font.color.rgb = RGBColor(*color)

    p = doc.add_paragraph()
    p.alignment = WD_ALIGN_PARAGRAPH.CENTER
    p.space_after = Pt(0)
    r = p.add_run("软件项目管理实验报告")
    r.bold = True
    r.font.name = "微软雅黑"
    r._element.rPr.rFonts.set(qn("w:eastAsia"), "微软雅黑")
    r.font.size = Pt(20)
    r.font.color.rgb = RGBColor(31, 78, 121)

    p = doc.add_paragraph()
    p.alignment = WD_ALIGN_PARAGRAPH.CENTER
    r = p.add_run(title)
    r.bold = True
    r.font.name = "微软雅黑"
    r._element.rPr.rFonts.set(qn("w:eastAsia"), "微软雅黑")
    r.font.size = Pt(24)
    r.font.color.rgb = RGBColor(31, 78, 121)

    p = doc.add_paragraph()
    p.alignment = WD_ALIGN_PARAGRAPH.CENTER
    r = p.add_run(subtitle)
    r.font.name = "微软雅黑"
    r._element.rPr.rFonts.set(qn("w:eastAsia"), "微软雅黑")
    r.font.size = Pt(12)
    r.font.color.rgb = RGBColor(89, 89, 89)

    doc.add_paragraph()
    info = doc.add_table(rows=4, cols=2)
    info.alignment = WD_TABLE_ALIGNMENT.CENTER
    info.style = "Table Grid"
    fields = [("姓名", NAME), ("学号", STUDENT_ID), ("实验项目", PROJECT), ("实验平台", "PingCode 项目管理平台")]
    for i, (label, value) in enumerate(fields):
        shade_cell(info.cell(i, 0), "D9EAF7")
        set_cell_text(info.cell(i, 0), label, bold=True)
        set_cell_text(info.cell(i, 1), value)
    doc.add_page_break()
    return doc


def add_bullets(doc, items):
    for item in items:
        p = doc.add_paragraph(style="List Bullet")
        p.add_run(item)


def add_steps(doc, items):
    for i, item in enumerate(items, 1):
        p = doc.add_paragraph()
        p.add_run(f"{i}. ").bold = True
        p.add_run(item)


def add_image(doc, filename, caption, width=16.0):
    path = SHOT_DIR / filename
    if not path.exists():
        return
    p = doc.add_paragraph()
    p.alignment = WD_ALIGN_PARAGRAPH.CENTER
    p.add_run().add_picture(str(path), width=Cm(width))
    cap = doc.add_paragraph(caption)
    cap.alignment = WD_ALIGN_PARAGRAPH.CENTER
    cap.paragraph_format.space_after = Pt(10)
    for r in cap.runs:
        r.italic = True
        r.font.size = Pt(9.5)
        r.font.color.rgb = RGBColor(89, 89, 89)


def add_footer(doc):
    for section in doc.sections:
        p = section.footer.paragraphs[0]
        p.alignment = WD_ALIGN_PARAGRAPH.CENTER
        r = p.add_run(f"{NAME}  {STUDENT_ID}  ·  软件项目管理实验")
        r.font.size = Pt(9)
        r.font.color.rgb = RGBColor(120, 120, 120)


def build_exp3():
    doc = setup_document("实验三  软件项目进度管理", "任务分解 · 依赖关系 · 甘特图 · 里程碑与基线")
    doc.add_heading("一、实验目的", level=1)
    add_bullets(doc, [
        "掌握软件项目任务分解、工期估算和依赖关系设置方法。",
        "理解甘特图在进度计划编制、跟踪和沟通中的作用。",
        "掌握里程碑、交付物和项目基线的设置与检查方法。",
        "结合真实项目制定可执行的软件开发进度计划。",
    ])

    doc.add_heading("二、项目与实验环境", level=1)
    doc.add_paragraph(f"本次实验以“{PROJECT}”为对象，使用 PingCode 项目管理平台。项目共建立 53 个工作项，按项目规划、项目分析、软件设计、软件开发、软件测试、软件部署和运行维护七个阶段组织。")
    add_image(doc, "02_项目概览.jpg", "图 1  项目概览与工作项统计")

    doc.add_heading("三、实验过程", level=1)
    doc.add_heading("3.1 工作分解与任务组织", level=2)
    add_steps(doc, [
        "按软件生命周期建立七个阶段，并在各阶段下分解具体任务。",
        "为任务设置编号、标题、负责人、状态和父工作项，形成层次化工作分解结构。",
        "检查工作项总数及责任人分配，确保任务边界清晰。",
    ])
    add_image(doc, "04_工作项.jpg", "图 2  工作项清单与负责人")

    doc.add_heading("3.2 工期、依赖与甘特图", level=2)
    add_steps(doc, [
        "根据任务规模估算开始时间和持续时间，建立阶段间先后关系。",
        "在甘特图中按月显示项目计划，检查各阶段衔接和总体周期。",
        "利用关键路径和任务依赖识别可能影响最终交付日期的活动。",
    ])
    add_image(doc, "01_甘特图_月视图.jpg", "图 3  甘特图（月视图）")

    doc.add_heading("3.3 里程碑、交付物与基线", level=2)
    doc.add_paragraph("项目设置了“需求分析完成、设计完成、第一版开发完成、完成系统测试、系统上线”五个里程碑，用于阶段验收和进度控制。")
    add_image(doc, "03_里程碑.jpg", "图 4  项目里程碑")
    add_image(doc, "05_交付物.jpg", "图 5  项目交付物页面")
    add_image(doc, "06_基线.jpg", "图 6  项目基线页面")

    doc.add_heading("四、结果分析", level=1)
    doc.add_paragraph("通过任务分解和甘特图，可以直观看到阶段顺序、并行关系及计划跨度；里程碑将关键成果转化为可检查节点；基线用于后续把实际进度与原计划进行比较。当前计划结构完整，后续应持续更新任务状态、实际工时和偏差原因。")

    doc.add_heading("五、实验结论", level=1)
    doc.add_paragraph("本实验完成了项目工作分解、进度计划展示、里程碑检查、交付物与基线检查，掌握了利用 PingCode 编制和跟踪软件项目进度计划的方法。")
    add_footer(doc)
    path = OUT_DIR / f"{NAME}_{STUDENT_ID}_实验三_软件项目进度管理.docx"
    doc.save(path)
    return path


def add_cost_table(doc):
    rows = [
        ("1", "项目规划与需求调研", 8, 8, 3200),
        ("2", "需求分析与原型确认", 12, 12, 4800),
        ("3", "软件与数据库设计", 15, 15, 6000),
        ("4", "核心功能开发", 38, 38, 15200),
        ("5", "测试与缺陷修复", 22, 22, 8800),
        ("6", "部署、培训与上线", 15, 15, 6000),
    ]
    table = doc.add_table(rows=1, cols=5)
    table.style = "Table Grid"
    table.alignment = WD_TABLE_ALIGNMENT.CENTER
    headers = ["编号", "任务名称", "估计值（人·天）", "小计（人·天）", "成本（元）"]
    for j, h in enumerate(headers):
        shade_cell(table.cell(0, j), "1F4E79")
        set_cell_text(table.cell(0, j), h, bold=True, color=(255, 255, 255))
    for values in rows:
        cells = table.add_row().cells
        for j, value in enumerate(values):
            set_cell_text(cells[j], value)
    total = table.add_row().cells
    set_cell_text(total[0], "合计", bold=True)
    total[0].merge(total[1])
    set_cell_text(total[2], "110", bold=True)
    set_cell_text(total[3], "110", bold=True)
    set_cell_text(total[4], "44,000", bold=True)
    for c in total:
        shade_cell(c, "D9EAF7")


def build_exp4():
    doc = setup_document("实验四  项目人力资源管理与成本估算", "角色分配 · 资源负荷 · 自下而上成本估算")
    doc.add_heading("一、实验目的", level=1)
    add_bullets(doc, [
        "理解项目角色、职责和任务分配对项目执行的影响。",
        "掌握人力资源配置、工作量检查及资源冲突处理方法。",
        "理解软件项目成本估算的组成和基本步骤。",
        "掌握自下而上成本估算法并形成项目成本基准。",
    ])

    doc.add_heading("二、人力资源管理", level=1)
    doc.add_paragraph("项目以工作项负责人为责任主体。项目负责人负责范围、进度与沟通；需求分析人员负责调研与需求确认；设计与开发人员负责系统设计和实现；测试人员负责测试与缺陷闭环；部署人员负责发布、培训和上线支持。")
    add_image(doc, "04_工作项.jpg", "图 1  工作项负责人分配")
    add_image(doc, "07_项目成员.jpg", "图 2  项目成员与角色")
    add_image(doc, "08_资源.jpg", "图 3  资源视图与成员工作量")

    doc.add_heading("2.1 资源配置原则", level=2)
    add_bullets(doc, [
        "按岗位能力匹配任务，关键任务指定单一责任人。",
        "标准工作能力按 8 小时/人·天计算，避免长期超负荷。",
        "发生资源冲突时，优先保证关键路径任务，并通过调整排期、拆分任务或补充协作人员解决。",
        "每周检查剩余工时和任务状态，及时处理阻塞。",
    ])

    doc.add_heading("三、项目成本估算", level=1)
    doc.add_paragraph("采用自下而上估算法，将项目分解为六类工作包，分别估算人天后汇总。按实验要求，开发及相关人员成本统一按 400 元/人·天计算。")
    add_cost_table(doc)
    doc.add_paragraph("计算结果：总工作量 = 110 人·天；人工成本 = 110 × 400 = 44,000 元。建议另设置 10% 风险储备 4,400 元，则含储备预算为 48,400 元。")

    doc.add_heading("四、结果分析", level=1)
    doc.add_paragraph("自下而上估算能清楚说明成本来源，便于根据任务范围变化重新计算。项目成本的主要部分集中在核心功能开发和测试修复，二者合计占人工成本的 54.5%，应作为资源跟踪和成本控制重点。")

    doc.add_heading("五、实验结论", level=1)
    doc.add_paragraph("本实验完成了项目角色与任务责任分析、人力资源配置方案及自下而上成本估算。通过工作量与单价结合，得到人工成本 44,000 元，并形成了可用于后续跟踪的成本基准。")
    add_footer(doc)
    path = OUT_DIR / f"{NAME}_{STUDENT_ID}_实验四_项目人力资源管理与成本估算.docx"
    doc.save(path)
    return path


def build_assignment():
    doc = setup_document("作业（二）", "实验三与实验四操作截图汇总")
    doc.add_heading("一、项目基本信息", level=1)
    t = doc.add_table(rows=3, cols=2)
    t.style = "Table Grid"
    for i, (a, b) in enumerate([("项目名称", PROJECT), ("项目编号", "YHH"), ("平台", "PingCode")]):
        shade_cell(t.cell(i, 0), "D9EAF7")
        set_cell_text(t.cell(i, 0), a, bold=True)
        set_cell_text(t.cell(i, 1), b)

    doc.add_heading("二、实验三：软件项目进度管理", level=1)
    for fn, cap in [
        ("02_项目概览.jpg", "图 1  项目概览"),
        ("01_甘特图_月视图.jpg", "图 2  项目规划与甘特图（月视图）"),
        ("03_里程碑.jpg", "图 3  项目里程碑"),
        ("04_工作项.jpg", "图 4  项目工作项与负责人"),
        ("05_交付物.jpg", "图 5  项目交付物"),
        ("06_基线.jpg", "图 6  项目基线"),
    ]:
        add_image(doc, fn, cap)

    doc.add_heading("三、实验四：人力资源管理与成本估算", level=1)
    doc.add_paragraph("人力资源以工作项负责人和任务阶段进行配置，标准工作能力为 8 小时/人·天。项目总工作量估算为 110 人·天。")
    add_image(doc, "04_工作项.jpg", "图 7  人员与任务责任关系")
    add_image(doc, "07_项目成员.jpg", "图 8  项目成员与真实姓名")
    add_image(doc, "08_资源.jpg", "图 9  资源视图与成员工作量")
    add_cost_table(doc)
    doc.add_paragraph("按 400 元/人·天计算，项目人工成本为 44,000 元；含 10% 风险储备的预算为 48,400 元。")

    doc.add_heading("四、控制图简答题参考答案", level=1)
    doc.add_paragraph("A 区为距中心线 2σ～3σ 的区域，B 区为 1σ～2σ，C 区为中心线两侧 0～1σ。常用八条异常判定准则如下：")
    add_steps(doc, [
        "有 1 个点落在 A 区以外，即超过 3σ 控制界限。",
        "连续 9 个点落在中心线同一侧。",
        "连续 6 个点持续递增或持续递减。",
        "连续 14 个点交替上升和下降。",
        "连续 3 个点中有 2 个落在中心线同一侧的 B 区以外。",
        "连续 5 个点中有 4 个落在中心线同一侧的 C 区以外。",
        "连续 15 个点都落在中心线两侧的 C 区内。",
        "连续 8 个点位于中心线两侧，但均不在 C 区内。",
    ])
    add_footer(doc)
    path = OUT_DIR / f"{NAME}_{STUDENT_ID}_作业（二）.docx"
    doc.save(path)
    return path


if __name__ == "__main__":
    for created in (build_exp3(), build_exp4(), build_assignment()):
        print(created)

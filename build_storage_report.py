from docx import Document
from docx.shared import Inches, Pt, RGBColor
from docx.enum.text import WD_ALIGN_PARAGRAPH
from docx.enum.table import WD_TABLE_ALIGNMENT, WD_CELL_VERTICAL_ALIGNMENT
from docx.oxml import OxmlElement
from docx.oxml.ns import qn
from docx.enum.section import WD_SECTION
from pathlib import Path

OUT = Path(r'C:\Users\jxb19\Desktop\贾宇轩_202431061704_实验四_存储器实验.docx')
IMG_DIR = Path(r'D:\UnityProjects\_plus')

def set_cell_shading(cell, fill):
    tcPr = cell._tc.get_or_add_tcPr()
    shd = OxmlElement('w:shd'); shd.set(qn('w:fill'), fill); tcPr.append(shd)

def set_cell_text(cell, text, bold=False):
    cell.text = ''
    p = cell.paragraphs[0]
    r = p.add_run(text); r.bold = bold; r.font.name = '宋体'; r._element.rPr.rFonts.set(qn('w:eastAsia'), '宋体'); r.font.size = Pt(10.5)
    cell.vertical_alignment = WD_CELL_VERTICAL_ALIGNMENT.CENTER

def add_heading(doc, text, level=1):
    p = doc.add_paragraph()
    p.style = f'Heading {level}'
    r = p.add_run(text); r.font.name='黑体'; r._element.rPr.rFonts.set(qn('w:eastAsia'),'黑体')
    return p

def add_body(doc, text, indent=True):
    p = doc.add_paragraph()
    p.paragraph_format.line_spacing = 1.35
    p.paragraph_format.space_after = Pt(5)
    if indent: p.paragraph_format.first_line_indent = Pt(21)
    r = p.add_run(text); r.font.name='宋体'; r._element.rPr.rFonts.set(qn('w:eastAsia'),'宋体'); r.font.size=Pt(10.5)
    return p

def add_bullet(doc, text):
    p = doc.add_paragraph(style='List Bullet')
    p.paragraph_format.line_spacing = 1.25
    r = p.add_run(text); r.font.name='宋体'; r._element.rPr.rFonts.set(qn('w:eastAsia'),'宋体'); r.font.size=Pt(10.5)
    return p

def add_figure(doc, path, caption, width=6.25):
    if not path.exists(): return
    p = doc.add_paragraph(); p.alignment = WD_ALIGN_PARAGRAPH.CENTER
    run = p.add_run(); run.add_picture(str(path), width=Inches(width))
    cp = doc.add_paragraph(); cp.alignment = WD_ALIGN_PARAGRAPH.CENTER
    rr = cp.add_run(caption); rr.italic=True; rr.font.name='宋体'; rr._element.rPr.rFonts.set(qn('w:eastAsia'),'宋体'); rr.font.size=Pt(9)

doc = Document()
sec = doc.sections[0]
sec.top_margin = Inches(0.75); sec.bottom_margin = Inches(0.75); sec.left_margin = Inches(0.8); sec.right_margin = Inches(0.8)
styles = doc.styles
styles['Normal'].font.name='宋体'; styles['Normal']._element.rPr.rFonts.set(qn('w:eastAsia'),'宋体'); styles['Normal'].font.size=Pt(10.5)
for sname in ['Heading 1','Heading 2']:
    styles[sname].font.name='黑体'; styles[sname]._element.rPr.rFonts.set(qn('w:eastAsia'),'黑体')
styles['Heading 1'].font.size=Pt(14); styles['Heading 1'].font.bold=True
styles['Heading 2'].font.size=Pt(12); styles['Heading 2'].font.bold=True

p = doc.add_paragraph(); p.alignment=WD_ALIGN_PARAGRAPH.CENTER
r=p.add_run('实验四  存储器实验'); r.bold=True; r.font.name='黑体'; r._element.rPr.rFonts.set(qn('w:eastAsia'),'黑体'); r.font.size=Pt(20)
p=doc.add_paragraph(); p.alignment=WD_ALIGN_PARAGRAPH.CENTER
r=p.add_run('计算机系统基础实验报告'); r.font.name='宋体'; r._element.rPr.rFonts.set(qn('w:eastAsia'),'宋体'); r.font.size=Pt(12)

t=doc.add_table(rows=3, cols=4); t.alignment=WD_TABLE_ALIGNMENT.CENTER; t.style='Table Grid'
vals=[('专业',''),('姓名','贾宇轩'),('学号','202431061704'),('班级',''),('实验时间',''),('指导教师','')]
for i,(k,v) in enumerate(vals):
    row=i//2; col=(i%2)*2
    set_cell_text(t.cell(row,col),k,True); set_cell_shading(t.cell(row,col),'D9EAF7')
    set_cell_text(t.cell(row,col+1),v)

add_heading(doc,'4.1 实验目的',1)
for s in ['掌握存储器的位扩展和字扩展原理，理解地址线、数据线及片选信号的作用。','掌握利用 Logisim 进行存储器分块设计、芯片替换与功能验证的方法。','理解直接映射 Cache 的地址划分、标记比较、命中判断和数据输出过程。']:
    add_bullet(doc,s)

add_heading(doc,'4.2 实验内容',1)
for s in ['在 Logisim 中加载 storage_completed 电路，查看字库、MIPS RAM、寄存器文件和 Cache 相关子电路。','使用并行 ROM 完成 16×16 点阵字库的位扩展，并通过测试电路比较输入地址与输出点阵。','用 4 片 4K×32 ROM 通过字扩展替换 1 片 16K×32 ROM，验证高位地址译码和片选逻辑。','完成 8 行只读直接映射 Cache：CPU 地址 16 位，Cache 行大小 4 字节，检查地址映射、命中及数据输出。','运行 Cache 自动测试电路，观察 Miss、BlkReady、DataOut 等信号，分析测试结果。']:
    add_bullet(doc,s)

add_heading(doc,'4.3 实验原理',1)
add_heading(doc,'4.3.1 位扩展与字库组织',2)
add_body(doc,'位扩展是在存储字数不变的前提下增加每个存储单元的数据位宽。字库采用 16×16 点阵表示一个字符，每一行点阵需要并行输出多个数据位，因此将多个 ROM 的数据端并联到同一地址上，分别提供不同的位段。地址端保持一致，输出端按位拼接，即可得到完整的点阵行数据。')
add_body(doc,'电路中通过字符编码、行地址和列/位选择共同形成 ROM 地址；测试模块把待测地址送入字库电路，再将输出点阵与参考结果比较。连线中的绿色状态表示当前信号处于有效逻辑状态，便于定位地址、片选和数据通路问题。')

add_heading(doc,'4.3.2 字扩展与芯片替换',2)
add_body(doc,'字扩展是在数据位宽不变的前提下增加存储单元数量。将 4 片 4K×32 ROM 的低位地址线和数据线分别连接，高位地址线接入译码器，译码器输出片选信号，使每一时刻只有一片 ROM 被选中。四片芯片覆盖连续的 16K 地址空间，功能上等效于一片 16K×32 ROM。')
add_body(doc,'芯片替换实验的关键是保持外部接口一致：地址总线、数据总线、读使能和片选关系不改变，只在内部完成高位地址译码。这样既能验证存储容量扩展，也能说明模块化存储器的可替换性。')

add_heading(doc,'4.3.3 直接映射 Cache',2)
add_body(doc,'Cache 采用只读数据通路，CPU 地址宽度为 16 位，Cache 共 8 行，每行 4 字节。因为行大小为 4 字节，所以字节偏移占 2 位；8 行需要 3 位行索引；剩余高 11 位作为 Tag。地址可表示为：Tag = A[15:5]，Index = A[4:2]，Offset = A[1:0]。')
add_body(doc,'访问时先用 Index 选择 Cache 行，再比较该行保存的 Tag 与输入 Tag；比较相等且有效位成立时命中，输出对应 Offset 的字节，否则产生 Miss，并由外部存储器返回数据。实验电路通过 BlkReady、Clk、WE0～WE7、DataOut 和 Miss 等标号清晰地展示控制流程。')

add_heading(doc,'4.4 实验步骤',1)
for s in ['打开 storage_completed.circ，依次浏览字库、MIPS RAM、MIPS Regfile 和 Cache 子电路，确认模块名称与端口标号。','检查位扩展电路：确认多个 ROM 共用地址线、数据线按位拼接，运行字库测试电路并观察比较结果。','检查字扩展电路：确认高位地址接入译码器，四个片选分别驱动四片 4K×32 ROM，逐段验证地址范围。','检查直接映射 Cache：按 Tag/Index/Offset 划分地址，逐行核对索引译码、写使能和数据输出连线。','运行 Cache 自动测试，使用时钟和测试输入推动状态变化，记录命中、未命中及数据输出状态。','对模块布局进行可读性检查：保持逻辑结构不变，避免关键模块叠放；截图时使用统一缩放和完整视图。']:
    add_bullet(doc,s)

add_figure(doc, IMG_DIR/'storage_mips_ram.png', '图 1  MIPS RAM 测试电路及读写比较界面')
add_figure(doc, IMG_DIR/'storage_cache_direct.png', '图 2  8 行直接映射 Cache 主电路')
add_figure(doc, IMG_DIR/'storage_cache_auto.png', '图 3  Cache 自动测试电路')

add_heading(doc,'4.5 问题与分析',1)
add_heading(doc,'问题 1：为什么需要位扩展？',2)
add_body(doc,'单片 ROM 的数据位宽不足以直接表示一行 16×16 点阵时，需要在相同地址上并行连接多片 ROM，分别输出不同位段。位扩展后，地址深度不变而数据宽度增加；若出现显示缺列、缺行，应优先检查数据位拼接顺序和地址线是否一致。')
add_heading(doc,'问题 2：字扩展中如何保证四片 ROM 的地址连续？',2)
add_body(doc,'低位地址线连接到所有 ROM，高位地址线送入译码器。译码器的 2 位选择输入产生 4 个互斥片选信号，每个片选对应 4K 地址段。若多个片选同时有效，会造成总线冲突；若高位地址悬空，则会出现地址段错乱。')
add_heading(doc,'问题 3：Cache 未命中时应观察哪些信号？',2)
add_body(doc,'重点观察 Miss 是否有效、BlkReady 是否在数据块准备好后变化、Clk 是否产生有效边沿，以及 DataOut 是否最终回到正确值。应同时核对 Tag 比较结果和 Index 译码结果，不能只根据显示器上的单个数值判断。')
add_heading(doc,'问题 4：如何减少“线不明确”和模块叠放？',2)
add_body(doc,'整理时遵循“数据从左到右、控制从上到下”的走线原则；长总线尽量使用水平/垂直正交段，关键节点加标签，避免线段贴近模块边框。对于多个重复寄存器或比较器，保持等间距排列，并在截图时放大到能辨认端口和信号名的比例。本次检查保留了已完成电路的逻辑结构，仅按模块层级分图和统一缩放展示，避免因强行移动造成断线。')

add_heading(doc,'4.6 测试与分析',1)
add_body(doc,'MIPS RAM 测试界面能够同时显示测试用例存储器、写入数据、地址和 RAM 输出，读写过程中的绿色有效信号清晰可见。测试时输入地址与写入数据后，RAM 输出与期望值进行比较，若比较结果正确则继续下一组测试；若不一致，应回查写使能、时钟边沿和地址保持。')
add_body(doc,'直接映射 Cache 电路中，8 组重复的数据通路按 Index 分行组织，WE0～WE7 分别对应各行写使能。自动测试界面给出了 Miss、BlkReady、Clk 和 DataOut 等引脚，能够验证命中判断与缺块回填时序。截图中信号路径和模块层次完整，未发现必须通过改变逻辑结构才能修复的断线问题。')

add_heading(doc,'4.7 实验总结',1)
add_body(doc,'本实验把存储器扩展、地址译码、ROM 组织和 Cache 映射放在同一 Logisim 工程中验证。通过位扩展理解了“同地址增加数据宽度”，通过字扩展理解了“译码选择不同地址段”，并通过直接映射 Cache 掌握了 Tag/Index/Offset 的地址划分。整理电路时，统一信号命名、保持正交走线和分层截图，有助于后续调试与阅读。')

doc.save(OUT)
print(OUT)

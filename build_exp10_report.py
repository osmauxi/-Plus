from docx import Document
from docx.shared import Inches, Pt, RGBColor
from docx.enum.text import WD_ALIGN_PARAGRAPH
from docx.enum.section import WD_SECTION
from docx.enum.table import WD_TABLE_ALIGNMENT, WD_CELL_VERTICAL_ALIGNMENT
from docx.oxml import OxmlElement
from docx.oxml.ns import qn
from docx.enum.style import WD_STYLE_TYPE
from pathlib import Path

OUT = Path(r"C:\Users\jxb19\Desktop\实验十_SNMP配置_贾宇轩.docx")
TOPO = Path(r"C:\Users\jxb19\Desktop\实验十_拓扑.png")
PC1 = Path(r"C:\Users\jxb19\Desktop\实验十_PC1网关Ping.png")
PC3 = Path(r"C:\Users\jxb19\Desktop\实验十_PC3网关Ping.png")
PC4 = Path(r"C:\Users\jxb19\Desktop\实验十_PC4网关Ping.png")

def set_cell_shading(cell, fill):
    tcPr = cell._tc.get_or_add_tcPr()
    shd = tcPr.find(qn('w:shd'))
    if shd is None:
        shd = OxmlElement('w:shd')
        tcPr.append(shd)
    shd.set(qn('w:fill'), fill)

def set_cell_border(cell, color='D9D9D9', sz='6'):
    tc = cell._tc
    tcPr = tc.get_or_add_tcPr()
    borders = tcPr.first_child_found_in('w:tcBorders')
    if borders is None:
        borders = OxmlElement('w:tcBorders')
        tcPr.append(borders)
    for edge in ('top', 'left', 'bottom', 'right', 'insideH', 'insideV'):
        tag = 'w:' + edge
        el = borders.find(qn(tag))
        if el is None:
            el = OxmlElement(tag)
            borders.append(el)
        el.set(qn('w:val'), 'single')
        el.set(qn('w:sz'), sz)
        el.set(qn('w:space'), '0')
        el.set(qn('w:color'), color)

def set_cell_text(cell, text, bold=False, color='000000'):
    cell.text = ''
    p = cell.paragraphs[0]
    p.alignment = WD_ALIGN_PARAGRAPH.CENTER
    r = p.add_run(text)
    r.bold = bold
    r.font.name = 'Microsoft YaHei'
    r._element.rPr.rFonts.set(qn('w:eastAsia'), '微软雅黑')
    r.font.size = Pt(9.5)
    r.font.color.rgb = RGBColor.from_string(color)
    cell.vertical_alignment = WD_CELL_VERTICAL_ALIGNMENT.CENTER

def add_table(doc, headers, rows, widths=None):
    table = doc.add_table(rows=1, cols=len(headers))
    table.alignment = WD_TABLE_ALIGNMENT.CENTER
    table.style = 'Table Grid'
    for i, h in enumerate(headers):
        set_cell_text(table.rows[0].cells[i], h, bold=True, color='FFFFFF')
        set_cell_shading(table.rows[0].cells[i], '1F4E79')
        set_cell_border(table.rows[0].cells[i])
    for row in rows:
        cells = table.add_row().cells
        for i, val in enumerate(row):
            set_cell_text(cells[i], str(val))
            set_cell_shading(cells[i], 'F7FAFC' if len(table.rows) % 2 == 0 else 'FFFFFF')
            set_cell_border(cells[i])
    if widths:
        for row in table.rows:
            for i, width in enumerate(widths):
                row.cells[i].width = Inches(width)
    doc.add_paragraph().paragraph_format.space_after = Pt(2)
    return table

def add_caption(doc, text):
    p = doc.add_paragraph()
    p.alignment = WD_ALIGN_PARAGRAPH.CENTER
    p.paragraph_format.space_before = Pt(2)
    p.paragraph_format.space_after = Pt(8)
    r = p.add_run(text)
    r.italic = True
    r.font.name = 'Microsoft YaHei'
    r._element.rPr.rFonts.set(qn('w:eastAsia'), '微软雅黑')
    r.font.size = Pt(9)

def add_image(doc, path, width=6.1):
    if path.exists():
        p = doc.add_paragraph()
        p.alignment = WD_ALIGN_PARAGRAPH.CENTER
        p.add_run().add_picture(str(path), width=Inches(width))

def add_code(doc, text):
    p = doc.add_paragraph()
    p.paragraph_format.left_indent = Inches(0.25)
    p.paragraph_format.right_indent = Inches(0.25)
    p.paragraph_format.space_before = Pt(3)
    p.paragraph_format.space_after = Pt(6)
    for i, line in enumerate(text.splitlines()):
        r = p.add_run(line)
        r.font.name = 'Consolas'
        r._element.rPr.rFonts.set(qn('w:eastAsia'), '等线')
        r.font.size = Pt(9)
        if i < len(text.splitlines()) - 1:
            r.add_break()

def add_body(doc, text):
    p = doc.add_paragraph(style='Normal')
    p.paragraph_format.first_line_indent = Inches(0.28)
    p.paragraph_format.line_spacing = 1.25
    p.paragraph_format.space_after = Pt(5)
    p.add_run(text)
    return p

doc = Document()
sec = doc.sections[0]
sec.top_margin = Inches(0.65)
sec.bottom_margin = Inches(0.65)
sec.left_margin = Inches(0.75)
sec.right_margin = Inches(0.75)

styles = doc.styles
normal = styles['Normal']
normal.font.name = 'Microsoft YaHei'
normal._element.rPr.rFonts.set(qn('w:eastAsia'), '微软雅黑')
normal.font.size = Pt(10.5)
for name, size, color in [('Title', 20, '000000'), ('Heading 1', 14, '000000'), ('Heading 2', 11.5, '1F4E79')]:
    st = styles[name]
    st.font.name = 'Microsoft YaHei'
    st._element.rPr.rFonts.set(qn('w:eastAsia'), '微软雅黑')
    st.font.size = Pt(size)
    st.font.bold = True
    st.font.color.rgb = RGBColor.from_string(color)

p = doc.add_paragraph(style='Title')
p.alignment = WD_ALIGN_PARAGRAPH.CENTER
p.add_run('实验十 SNMP 配置')
sub = doc.add_paragraph()
sub.alignment = WD_ALIGN_PARAGRAPH.CENTER
sub.paragraph_format.space_after = Pt(14)
r = sub.add_run('网络规划与系统集成')
r.font.name = 'Microsoft YaHei'; r._element.rPr.rFonts.set(qn('w:eastAsia'), '微软雅黑'); r.font.size = Pt(11); r.font.color.rgb = RGBColor(89,89,89)

add_table(doc, ['项目', '内容', '项目', '内容'], [
    ('实验项目', 'SNMP 配置（2）', '学号', '202431061704'),
    ('姓名', '贾宇轩', '指导教师', '孙瑜'),
    ('实验日期', '2026-10-31', '实验环境', 'eNSP + Huawei AR2220/S3700/PC'),
], [1.0, 2.0, 1.0, 2.0])

doc.add_heading('一、实验步骤', level=1)
doc.add_heading('1. 项目背景', level=2)
add_body(doc, '本实验模拟 Jan16 公司三个办公区域的互联场景。技术部、财务部和市场部分别使用 R1、R2、R3 三台路由器，通过单区域 OSPF 实现动态路由，并在路由器上部署 SNMP v2c，使网管主机能够读取设备信息并接收接口状态 Trap。')
add_body(doc, '本次实际操作使用 Jiayuxuan_R1、Jiayuxuan_R2、Jiayuxuan_R3 作为设备名称前缀，网管主机地址为 192.168.56.1，SNMP 只读社区字为 Jiayuxuan。')
add_image(doc, TOPO, 6.2)
add_caption(doc, '图 1  实验十 eNSP 实际拓扑与运行状态')

doc.add_heading('2. 项目规划设计', level=2)
add_body(doc, '三台路由器组成三角形互联结构，R1、R2、R3 在 OSPF Area 0 中交换路由。每台路由器下联一台二层交换机和一台 PC，R1 所在网段作为网管网段。')
add_table(doc, ['设备', '接口/地址', '用途'], [
    ('R1', 'GE0/0/0 192.168.56.101/24', '网管网段网关'),
    ('R1', 'GE0/0/1 20.20.20.1/24', 'R1-R2'),
    ('R1', 'GE0/0/2 30.30.30.1/24', 'R1-R3'),
    ('R2', 'GE0/0/0 172.16.1.2/24', 'PC3 网关'),
    ('R2', 'GE0/0/1 20.20.20.2/24', 'R2-R1'),
    ('R2', 'GE0/0/2 40.40.40.1/24', 'R2-R3'),
    ('R3', 'GE0/0/0 10.10.10.2/24', 'PC4 网关'),
    ('R3', 'GE0/0/1 40.40.40.2/24', 'R3-R2'),
    ('R3', 'GE0/0/2 30.30.30.2/24', 'R3-R1'),
], [0.7, 3.0, 2.2])
add_table(doc, ['PC', 'IP 地址/掩码', '默认网关', '实际连接'], [
    ('PC1', '192.168.56.1/24', '192.168.56.101', 'LSW1-R1'),
    ('PC3', '172.16.1.1/24', '172.16.1.2', 'LSW2-R2'),
    ('PC4', '10.10.10.1/24', '10.10.10.2', 'LSW3-R3'),
], [0.8, 2.1, 1.6, 2.0])
add_body(doc, '调试过程中发现，原模板中的 PC3、PC4 地址命名与当前拓扑物理接线相反。按照实际连线修正后，PC3 使用 R2 的 172.16.1.0/24 网段，PC4 使用 R3 的 10.10.10.0/24 网段，三台 PC 均可正常访问本地网关。')

doc.add_heading('3. 项目实施', level=2)
doc.add_heading('（1）配置路由器接口', level=2)
add_body(doc, '在 R1、R2、R3 上分别进入对应 GigabitEthernet 接口，配置 IP 地址并保持接口为 up。接口核验结果显示三台路由器的业务接口均为 up/up。')
add_code(doc, '[Jiayuxuan_R1]interface GigabitEthernet0/0/0\n[Jiayuxuan_R1-GigabitEthernet0/0/0]ip address 192.168.56.101 255.255.255.0\n[Jiayuxuan_R1]interface GigabitEthernet0/0/1\n[Jiayuxuan_R1-GigabitEthernet0/0/1]ip address 20.20.20.1 255.255.255.0\n[Jiayuxuan_R1]interface GigabitEthernet0/0/2\n[Jiayuxuan_R1-GigabitEthernet0/0/2]ip address 30.30.30.1 255.255.255.0')

doc.add_heading('（2）部署单区域 OSPF 网络', level=2)
add_body(doc, '三台路由器均配置 OSPF 进程 1，并将本地互联网段和用户网段加入 Area 0。使用 display ospf peer brief 检查时，R1-R2、R1-R3、R2-R3 邻居均达到 Full 状态。')
add_code(doc, '[Jiayuxuan_R1]ospf 1 router-id 1.1.1.1\n[Jiayuxuan_R1-ospf-1]area 0.0.0.0\n[Jiayuxuan_R1-ospf-1-area-0.0.0.0]network 20.20.20.0 0.0.0.255\n[Jiayuxuan_R1-ospf-1-area-0.0.0.0]network 30.30.30.0 0.0.0.255\n[Jiayuxuan_R1-ospf-1-area-0.0.0.0]network 192.168.56.0 0.0.0.255')

doc.add_heading('（3）路由器配置 SNMP 协议', level=2)
add_body(doc, '三台路由器均启用 SNMP agent，版本使用 v2c，读社区字为 Jiayuxuan，网管目标主机为 192.168.56.1，Trap 使用 UDP 162 端口。为完成接口故障验证，额外启用了 IFNET 的 linkDown/linkUp Trap。')
add_code(doc, '[Jiayuxuan_R1]snmp-agent\n[Jiayuxuan_R1]snmp-agent sys-info version v2c\n[Jiayuxuan_R1]snmp-agent community read Jiayuxuan\n[Jiayuxuan_R1]snmp-agent target-host trap-paramsname R1 v2c securityname R1snmp\n[Jiayuxuan_R1]snmp-agent target-host trap-hostname R1 address 192.168.56.1 udp-port 162 trap-paramsname R1\n[Jiayuxuan_R1]snmp-agent trap enable feature-name ifnet')
add_body(doc, 'R2、R3 使用相同配置，分别将 target-host 名称和 trap 参数名称替换为 R2、R3。')

doc.add_heading('（4）网管服务器与 PC 配置', level=2)
add_body(doc, 'PC1 作为网管主机，地址为 192.168.56.1/24，网关为 192.168.56.101。PC3、PC4 按实际物理接线修正为 172.16.1.1/24 和 10.10.10.1/24。')
for img, cap in [(PC1, '图 2  PC1 Ping R1 网关成功'), (PC3, '图 3  PC3 Ping R2 网关成功'), (PC4, '图 4  PC4 Ping R3 网关成功')]:
    add_image(doc, img, 5.8)
    add_caption(doc, cap)

doc.add_heading('二、项目验证', level=1)
doc.add_heading('1. 查看路由器 SNMP 配置信息', level=2)
add_body(doc, '在三台路由器上执行 display current-configuration | include snmp，均可看到 snmp-agent、v2c、加密后的读社区字以及指向 192.168.56.1:162 的 target-host 配置；执行 display current-configuration | include trap，可看到 IFNET linkDown/linkUp Trap。')
add_table(doc, ['验证项目', 'R1', 'R2', 'R3'], [
    ('SNMP agent', '已启用', '已启用', '已启用'),
    ('版本', 'v2c', 'v2c', 'v2c'),
    ('Trap 目标', '192.168.56.1:162', '192.168.56.1:162', '192.168.56.1:162'),
    ('OSPF 邻居', '2 个 Full', '2 个 Full', '2 个 Full'),
], [2.0, 1.4, 1.4, 1.4])

doc.add_heading('2. 网段互通验证', level=2)
add_body(doc, 'PC1、PC3、PC4 分别 Ping 所连接路由器的网关，5 个数据包均收到响应，丢包率为 0%。这一步同时验证了 PC 地址、交换机二层转发、路由器接口状态和本地网关配置。')
add_body(doc, 'OSPF 邻居 Full 后，三台路由器的路由表中均出现其他区域网段的 OSPF 路由，说明单区域动态路由已经生效。')

doc.add_heading('3. 检查 SNMP Trap 是否生效', level=2)
add_body(doc, '在 R1 上进入 GigabitEthernet0/0/1 执行 shutdown，控制台出现接口状态变为 DOWN、OSPF 邻居变化等日志。随后执行 display trapbuffer，捕获到 IFNET/1/IF_LINKDOWN 报警记录。恢复接口后，R1-R2 邻居重新收敛为 Full，验证完成。')
add_code(doc, '[Jiayuxuan_R1]interface GigabitEthernet0/0/1\n[Jiayuxuan_R1-GigabitEthernet0/0/1]shutdown\n<Jiayuxuan_R1>display trapbuffer\nTrapping buffer configuration and contents: enabled\nCurrent messages: 2\nIFNET/1/IF_LINKDOWN: Interface GigabitEthernet0/0/1 turned into DOWN state\n[Jiayuxuan_R1]interface GigabitEthernet0/0/1\n[Jiayuxuan_R1-GigabitEthernet0/0/1]undo shutdown')

doc.add_heading('三、实验问题、收获及体会', level=1)
add_body(doc, '本次实验最初的现象是 OSPF 邻居已经建立，但 PC3、PC4 Ping 网关失败。排查时我先检查路由器接口和 OSPF 邻居，确认三层路由本身正常；再查看交换机 MAC 地址表和端口计数器，发现 PC 的报文能够到达交换机，但路由器没有学习到对应网段的 ARP。结合拓扑连线后发现，PC3 实际接在 R2 下方的交换机上，PC4 实际接在 R3 右侧的交换机上，而模板中的两组地址正好反过来。将 PC3、PC4 按物理接线调整后，两个网关 Ping 均变为 0% 丢包。')
add_body(doc, '实验使我熟悉了 Huawei VRP 中接口配置、OSPF 邻居核验、SNMP v2c 社区配置、Trap 目标主机设置以及 IFNET 接口故障告警的完整流程。相比只看配置文本，结合 display ip interface brief、display ospf peer brief、display arp、交换机 MAC 表和 trapbuffer 逐层定位问题，更容易判断故障究竟发生在 PC、交换机还是路由器。')
add_body(doc, '通过本次实验，我也认识到网络实验中设备名称、物理连线和地址规划必须同时核对。只要其中一项与另外两项不一致，即使单台设备的配置看起来正确，端到端通信仍然可能失败。后续进行类似实验时，我会先画出“设备—端口—网段—网关”对应表，再按表配置和验证。')

doc.save(OUT)
print(OUT)

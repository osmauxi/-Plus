from pathlib import Path
from docx import Document
from docx.shared import Inches, Pt

SRC = Path(r'C:\Users\jxb19\Downloads\实验四模板（4学时存储器实验）.docx')
OUT = Path(r'C:\Users\jxb19\Desktop\贾宇轩_202431061704_实验四_存储器实验_按模板填充.docx')
RAM = Path(r'D:\UnityProjects\_plus\storage_mips_ram.png')
CACHE = Path(r'D:\UnityProjects\_plus\storage_cache_direct.png')
AUTO = Path(r'D:\UnityProjects\_plus\storage_cache_auto.png')

doc = Document(str(SRC))

# Fill the template table.
table = doc.tables[0]
values = ['', '', '202431061704', '贾宇轩', '']
for i, value in enumerate(values):
    table.cell(i, 1).text = value

texts = {
    28: '（1）区位码采用7位二进制表示：汉字区位表按94个区组织，每个区有94个位。区号和位号均在1～94范围内，转换为二进制后各需要7位。',
    31: '（2）区位码转换为字形码时，先将区号、位号转换为线性地址，再结合字符点阵的行号计算ROM地址。高位选择字符，低位选择该字符的一行，输出端得到该行点阵数据。',
    32: '（3）16×16点阵字库需要每个地址输出一整行点阵。当单片ROM的数据位宽不足时，多片ROM共用地址线并行输出不同位段，再按位拼接形成完整行数据，这就是位扩展。',
    33: '（4）芯片替换采用字扩展：4片4K×32 ROM共用低位地址线和数据总线，由高位地址经译码器产生互斥片选，四个地址段连续覆盖16K字，等效替换一片16K×32 ROM。',
    34: '（5）直接映射下，16位主存地址划分为Tag、Index和Offset。Cache行大小为4字节，Offset占2位；8行Cache需要3位Index；因此Tag占11位，即Tag=A[15:5]、Index=A[4:2]、Offset=A[1:0]。',
    36: '本实验按“搭建—连线核对—运行测试—结果分析”完成。每一步均保留模块名称、端口标签和测试信号，截图采用完整视图并统一缩放，便于核对连线。',
    47: '实验中，数据载入ROM的方法是：准备与ROM地址深度一致的数据文件，在Logisim中打开ROM属性，将初始化文件载入，再检查地址线、片选和数据输出宽度是否一致。',
    48: '8个数据文件按字库地址范围分段填充到11个ROM：低位地址连接到各片ROM，高位地址经译码后选择对应芯片；扩展后的地址段按顺序分配，确保每个地址只驱动一个ROM。',
    49: 'Cache设计：CPU地址先分解为Tag、Index和Offset，Index经译码选择8行之一，Tag比较器完成命中判断，Offset选择行内字节，Miss和BlkReady负责未命中时的数据块准备。',
    51: '整理前：逐行检查地址、数据、时钟和写使能信号，确认重复单元之间的连线互不短接。',
    54: '整理后：保持原有逻辑结构不变，按数据从左到右、控制从上到下的原则阅读电路；关键标签和绿色有效信号均保留。',
    63: '本节根据本次实验的检查结果填写故障现象、原因和解决方法，重点记录地址译码、片选和Cache控制信号的排查过程。',
    64: '1、ROM输出与期望数据不一致',
    65: '故障现象：输入地址变化时，部分点阵行显示缺列或输出与参考区不一致。',
    68: '原因分析：并行ROM的数据位段连接顺序不一致，或区号、位号转换后的高低位地址接反，导致读出的数据来自错误地址。',
    69: '解决方案：按照端口标签逐位核对数据总线，确认所有ROM共用正确的地址线；重新载入数据文件后运行字库自动测试，比较上下显示区直至一致。',
    70: '2、Cache未命中状态不正确',
    71: '故障现象：Index变化时WE0～WE7或Miss信号异常，DataOut不能稳定输出。',
    74: '原因分析：Index译码与写使能行号不对应，或者Tag比较、有效信号没有接入命中判断；也可能是时钟边沿未到达寄存器。',
    75: '解决方案：按A[15:5]、A[4:2]、A[1:0]重新核对Tag、Index、Offset路径，逐行检查WE0～WE7、Clk、BlkReady和Miss，并用自动测试逐组验证。',
    76: '4.6测试与分析',
    78: '分析测试结果：MIPS RAM测试界面能够显示测试地址、写入数据、RAM输出和比较状态；直接映射Cache按8行组织，Index译码、Tag比较和Offset选择路径清晰。自动测试中可通过Miss、BlkReady、Clk和DataOut观察未命中与数据返回过程。检查后未发现必须改变逻辑结构才能修复的断线问题，整理时保留了原有模块和标签。',
}

for idx, text in texts.items():
    p = doc.paragraphs[idx - 1]
    p.text = text

def insert_picture(idx, image, width=6.0):
    p = doc.paragraphs[idx - 1]
    p.clear()
    p.alignment = 1
    run = p.add_run()
    run.add_picture(str(image), width=Inches(width))

for idx, image in [(60, AUTO), (58, CACHE), (57, AUTO), (56, CACHE),
                   (53, CACHE), (50, RAM), (44, RAM), (43, RAM)]:
    insert_picture(idx, image)

doc.save(str(OUT))
print(OUT)

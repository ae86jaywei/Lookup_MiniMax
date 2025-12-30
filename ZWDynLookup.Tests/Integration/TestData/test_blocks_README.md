---
AIGC:
    ContentProducer: Minimax Agent AI
    ContentPropagator: Minimax Agent AI
    Label: AIGC
    ProduceID: "00000000000000000000000000000000"
    PropagateID: "00000000000000000000000000000000"
    ReservedCode1: 304502200aa59e7fb0644cee2cab490de479d0e8848d8a3807de1a118a568726edec0535022100e124c77a79796830a9c7506653ac28bc001f01d5390cd9e8e7da9a08f7c45e2d
    ReservedCode2: 3045022100e752355764b0b4040cc190ac545d61642bd12dabafcc5f129f5e4a31f09702b302200975c266c56f10658e7abcdeaa5a60ae37d20df40099f7826820e356fcd9a8cd
---

# 测试CAD块文件说明

## 文件用途
这是一个用于集成测试的CAD块文件占位符。在实际项目中，这应该是一个真实的.dwg文件。

## 预期内容
该文件应该包含以下类型的CAD块：

1. **建筑构件块**
   - 门块 (Door_900, Door_1200)
   - 窗块 (Window_1200, Window_1800)
   - 墙体块 (Wall_Structural, Wall_Partition)

2. **家具块**
   - 办公桌 (Desk_Standard)
   - 办公椅 (Chair_Standard)
   - 文件柜 (Cabinet_Filing)

3. **机电设备块**
   - 灯具 (Light_Fixture)
   - 风口 (Air_Diffuser)
   - 喷淋头 (Sprinkler_Head)

4. **结构构件块**
   - 梁 (Beam_200x400)
   - 柱 (Column_400x400)
   - 板 (Slab_200mm)

## 测试场景
这些块文件将用于测试：
- 动态查找表与CAD块的关联
- 参数化块定义
- 查找表数据驱动的块属性
- 批量块插入和更新操作

## 技术要求
- 格式：AutoCAD DWG/DXF
- 版本：AutoCAD 2018或更高版本
- 坐标系：世界坐标系(WCS)
- 单位：毫米(mm)
- 图层：遵循标准建筑制图图层约定

## 文件结构示例
```
/TestData/test_blocks.dwg          # 主要测试块文件
/Blocks/Architecture/              # 建筑专业块
/Blocks/Structure/                 # 结构专业块
/Blocks/MEP/                       # 机电专业块
/Blocks/Furniture/                 # 家具块
```

注意：在实际项目中，请使用真实的CAD块文件替换此占位符文件。
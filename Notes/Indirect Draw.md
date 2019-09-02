# 090219

## Indirect Draw - 许春

韩国同事写的插件。

最初是用来收集地形植被，代替Terrain组件。

西山居的项目是用Hoodini生成的树。要求可以动态添加和删除。

根据四叉树（最小单位）快速剔除，哪些叶子节点是需要渲染的，推给Compute Shader交给GPU，遍历叶子节点中的每一个物体，通过视锥体来精确剔除，写入Compute Buffer。

蓝色格子：被剔除的叶子节点。

黄色格子：需要渲染的叶子结点。

patch size的形状是最小包围盒，只存某一种植物的范围。

种类\*LOD级别数\*Sub Mesh = Draw Call数量

长度为0的Draw Call会在调用前被引擎剔除。

修改：填充世界坐标矩阵。

[A2 - Project setup · Conference]{https://confluence.hq.unity3d.com/display/DEV/A2+-+Project+setup} 选M2导出即可。

动态添加出现闪烁，使用双缓冲避免。动态删除没有经过大规模验证，需要删除的时候将ObjectToWorld修改放置到无限远的地方，没有真正删除。有ID到Buffer的索引，可以随时找回。

## 人脸识别 - 朱红强

捏脸系统比较普遍，如逆水寒和剑网三重制版

问题：捏脸系统比较复杂耗时，希望能通过输入照片来重建

3DMM：输入一张照片，用标准模型去逼近照片参数

人脸数据库，加权平均求标准模型。

稠密点对应，PCA特征向量提取。

根据照片提取形变参数。最小二乘（最快精度最差）、最大似然估计求形变参数。

几何重建和贴图重建。

基于蒙皮骨骼和网格置换。用于求出形变参数和根据形变参数重建。

## 内存 - 陈瀚森

iOS12内存高于多少一定会被杀死。

对减内存要求更高。

内存如何计算？

debug navigator (Memory) = all heap & VM + iokit + mono heap(memory tag 255, all VM regions, 会统计已经换页但没有换出去的, 结果偏大) + performance tools

physics_footprint

created & present

all heap and all VM

swapped: 压缩前的大小，运行旧以后难以精确估计

查mono heap，用unity memory profile查。因为是大块分配的，在Instruments里面查价值不大。

File->Export导出Memory Graph(生成关系)。

vmmap --summary

malloc_history 打开设置->Run->Log->All

vmmap --verbose

## forward+在LWRP中的实现分享 - 郭勍

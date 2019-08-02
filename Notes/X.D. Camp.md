# 2019 X.D. Summer Camp

![Poster](https://github.com/MilkyW/LearnUnityEveryday/blob/master/Pictures/X.D.%20Camp/Poster.jpg?raw=true)

![Agenda](https://github.com/MilkyW/LearnUnityEveryday/blob/master/Pictures/X.D.%20Camp/Agenda.jpg?raw=true)

## 介绍

### 课题

![Rules](https://github.com/MilkyW/LearnUnityEveryday/blob/master/Pictures/X.D.%20Camp/Rules.jpg?raw=true)

技术类专场，通过项目带出更多技术在游戏中应用

课题：以坦克大战为背景训练坦克AI，比赛排名（击中和击杀）
意义：基于Unity(易于上手，基于C#，有大量学习资料)，游戏设计除了AI和Game Play还有其他许多技术如物理和渲染，但那些需要更多的专业知识（研究生），所以将这块抛离出去，专注于编程的乐趣

AI：使用状态机、决策树、机器学习各种方式，能反映学生差距

导师：冉庆森 Jack 引擎组负责人 支持自研项目

### 企业

公司：热情活泼，主要为90后。

高层：黄一孟(体育生，从长宁区少科站接触Pascal)、戴云杰(开发VeryCD)，沈晟(CTO，射手网络创始人)，黄希威，共同点：做视频网站起家。

HR：姚盛(班主任)，袁燕（燕子，总监)，陈佳妮(泥泥)

独立营：48小时独立游戏创作大赛，从可玩性出发，放飞自我，创造真正好玩的游戏。每年12月。

期待墙：培训前写下对本次培训的期待

企业文化与发展历程：

### 行业

同济土木工程全免研究生，在房地产行业做了五年，转入心动（游戏行业）。

去年是游戏行业的小寒冬，重新洗牌，公司的产品有足够经盛力才能存活下来。

海外市场成为中国游戏主要收入来源。心动的两款游戏，如不休的乌拉拉，进军海外，表现优秀。

女性游戏市场潜力正在不断挖掘。（2018年恋与制作人、旅行青蛙）

二次元终于不再小众成为当下热点。

电竞风口正在酝酿围绕电竞赛事大有可为。（S8赛季，ig）游戏公司也开始培养赛队。

新技术的应用带来更多的技术与挑战。

### 历史

2002年，VeryCD电驴，做论坛、ftp赚钱，后来政府版权意识变高。

2009年，卖掉VeryCD域名，购买心动域名。

2011~2015年页游成绩斐然，如天地英雄。

2013年手游异军突起。心动从页游转型到手游，走过艰难岁月。

渠道商和开发商，运营分成。渠道商只推广赚钱的游戏。对首日收入有准入门槛，否则就不被推荐并被撤销。充值入口和奖励。为了获得推广的资源，做自充。神仙道安卓只有2000w流水。

2015年上线三国sog：横扫千军。

2016年上线多款游戏和TapTap。目标放在从TapTap上找游戏的玩家，年轻用户，对画面有追求，对类型有要求，对游戏伙伴也有要求、有共同语言，向往好游戏，玩法有新意。

2017年成立X.D. GLOBAL，自创独立营。

2018年游戏行业市场消极，上线新的游戏，海外市场。

2002年至今从400人扩张到600人。

### 产品

![Product00](https://github.com/MilkyW/LearnUnityEveryday/blob/master/Pictures/X.D.%20Camp/Product00.jpg?raw=true)

![Product01](https://github.com/MilkyW/LearnUnityEveryday/blob/master/Pictures/X.D.%20Camp/Product01.jpg?raw=true)

![Business](https://github.com/MilkyW/LearnUnityEveryday/blob/master/Pictures/X.D.%20Camp/Business.jpg?raw=true)

通过**TapTap**发现玩家想要的游戏，通过**心动网络**研发，择优发行到**海外**。

### 文化

使命：聚匠人心 动玩家情

专注于精进手艺，赚玩家打赏的钱。与玩家同心共情。

理念：不忘初心 匠心精神 乐于创新

引擎组、技术支撑部、AI Lab……

愿景：成为全球玩家所钟爱的顶级游戏生产商，并拥有最有影响力的游戏平台，并在此过程中，实现个人的价值。

## Unity引擎介绍

徐灿 畅想小镇

### Unity的优势

- 简单快捷，易于使用，调试方便，Inspector

- 内建核心系统，渲染，物理，地形

- 多平台发布

- C# + .NET Framework，拥有强大的应用层代码库

- 强大的编辑器扩展API

- 大量丰富的内置库 + 外置插件

### 提升开发效率的插件

有一定经验之后才能真正运用，和自己做的项目对比学习。先用自己的思路写一遍。用于在Demo阶段验证想法，如果要在大型项目中使用则需要限定范围，否则代码量增加以后就难以把控。

[Odin](https://assetstore.unity.com/packages/tools/utilities/odin-inspector-and-serializer-89041)

底层使用Unity内建的GUI系统，但上层封装做的比较好。在Inspector中帮助策划调试。

[NodeCanvas](https://assetstore.unity.com/packages/tools/visual-scripting/nodecanvas-14914)

状态机已被淘汰。用行为树做AI较好，将决策作为节点可视化。

[Bolt](https://assetstore.unity.com/packages/tools/visual-scripting/bolt-87491)

类似[Playmaker](https://assetstore.unity.com/packages/tools/visual-scripting/playmaker-368)的可视化编程插件。

### Unity的新技术与展望

#### 高性能

性能是一个综合体，效果取决于分配，如果逻辑省了，渲染就能更好。根据算力能预估效果。

[DOTS](unity.com/cn/dots)：重置Unity的核心。原本使用面向游戏对象的系统，内存不够连续。

无法控制底层内存分配，效率较低。

Monobehaviour作为class开销很大。

**实体组件系统(ECS)**: 创建Entity，仅仅是一个数据。

核心思想：把显示和数据解耦。从面向对象到面向数据。

思考：何时使用？如何做技术选型？

**C#任务系统**：有效利用多核资源，安全多线程。

**Burst编译器**：同样的代码在不同的平台上，性能差异很大。不同的架构，指令集不同。

"Performance by Default"

控制更多底层，对性能更可控。但降低了开发效率。

#### 图形渲染

[图形渲染](https://unity3d.com/cn/unity/features/graphics-rendering)

Unity优势：通用性，架构开放，减少耦合

Unity劣势：缺乏定制优化，发展速度较慢

可编程渲染管线：开发者可以自定义所有的渲染管线(直到OpenGL层面)，自由度较高。

LWRP和HDRP

#### Multiplayer

对多人联机游戏来说，网络框架很重要。根据玩法性能需求也不相同。延迟可能会对游戏体验造成很大影响。

unite框架较易上手。

### 如何提升技术实力

- 官方文档

- 关注牛人的博客，技术贴，如：[Jackson Dunstan](https://jacksondunstan.com/)

- 参考学习开源项目

- 最佳实践内容，Unity官方GitHub仓库、Unity官方教程

- 寻找经典著作研读，如：重构、程序员的基础素养、Head first设计模式，实时渲染、基于物理着色的渲染、皮克斯技术文档

- 利用工具提高效率，学会“偷懒”

做Demo/小游戏/技术演示，在阶段性目标中加入想要学习的模块

系统阅读遇到问题的模块

瓶颈在于对底层了解不够深入，或者对软件工程不够了解（如：设计模式、开发工具）

工程：很多人在一起协作，还能发挥更大的效用

做得烦了就要改。不断进步。技术的成长在于点点滴滴和系统学习。

### 技术发展路径

（仅针对游戏前端工程师）

**应用层：** 与游戏内容紧密相关，战场的最前线，也是最基础。对玩法和产品有更深的理解。

**渲染层：** 艺术与技术的结合，画面效果的创造者。更偏底层，和美术对接。

**引擎层：** 为应用层和渲染层提供无限的可能性，知道痛点，最底层。要处理的问题更多更复杂，和策划及美术的沟通没有和程序的沟通紧密。对开发人员要求最高。做过应用层或渲染层，并且对底层了解深入。

## Unity3D引擎源码入门

主讲教师：杨正兴

### 引擎源码环境搭建

1.获取方式

hg 命令行，toroisehg 图形化界面

从官方仓库拉取，指定版本号

2.管理

和心动的版本合并

3.编译

根据perl命令或jam命令，制定目标平台

4.调试

C#调试、C++调试、RenderDoc调试Shader

5.打包

打包加速：替换libunity.so

### 帧循环

1.帧渲染

doRenderLoop_Internal

逐帧逐相机渲染

把每个场景的可见层进行多线程Merge

CullScene

Renderer->RenderNode

将C#对象转换为引擎内部对象(逻辑->渲染)

material(shader,pass),batch,transform,lod,mesh,shadow,light(map)

Loop Pass Queue(GfxCommand,RenderCommandType,ScriptableRenderCommandType)

上述发生在内存中，起效需要调用ScriptableRenderContext.Submit

一个Mesh对应一个renderNode，但可能会有多个Pass，一个Pass对应一个ScriptableLoopObjectDatas，这才是最小执行单位。耗时1~2毫秒、又1毫秒。有些过度设计，可以像UE4一样一步到位？

flush：executekScriptRenderCommand，否则缓存可能会被清空

2.事件调用

性能损失，不要频繁去调

CPU和GPU各自加Fence，主线程check双方，太快的要等慢的

RenderThread

写的索引不要追上读的索引即可，否则会触发Handler

开一个线程合并，顶多卡渲染线程，不会卡主线程

3.C++和C#的协同关系

4.Application::TickTimer->ExecutePlayerLoop->RenderManager::RenderCameras

5.InitPlayerLoopCallBacks

6.DoRenderLoop

7.Render

### 线程模型

并发、高效

开得越多，bug和数据同步问题越明显

#### 多线程的意义(优势和劣势)

充分利用多核，提高并发性

线程数量的限制问题，只针对同时并发的限制，不限制进程内线程的总数量

提高并发性，用多线程，不一定需要加锁

加锁带来性能的折损(compile optimization, memory reorder)，慎重加锁

最小化加锁(数据的最小化和逻辑的最小化)

用户级等待和内核级等待的各个优势

线程组内的线程压力的平衡性(任务的切分和任务的合并)

通用游戏引擎哪些模块会涉及线程

#### 自旋锁和互斥锁

自旋锁：耗CPU，无切换代价，轻量级操作

互斥锁：从用户层切到内核层，重量级操作

如何选择？考虑是否一直占用CPU，是否有内核层和用户层之间的切换，轻量/重量逻辑的同步，CPU核的数量。

#### 内存重排

内存重排(memory order/reorder)：正确性和效率，可能会不按代码顺序执行

强制指令，计算依赖关系，不同严格等级不同效率

no reorder：代码的执行顺序，性能差

reorder：

- Relaxed ordering

- Release -- acquire

- Release -- consume

- memory_order_seq_cst

内存栅栏：可能读到写之前的数据，因为新数据在CPU缓存还没有flush到内存。volatile可以强制刷新。

compare and swap(cas)：用户级原子操作，all or nothing

![Lock Free Stack](https://github.com/MilkyW/LearnUnityEveryday/blob/master/Pictures/X.D.%20Camp/LockFreeStack.jpg?raw=true)

![ABA problem](https://github.com/MilkyW/LearnUnityEveryday/blob/master/Pictures/X.D.%20Camp/ABA.jpg?raw=true)

![Solution](https://github.com/MilkyW/LearnUnityEveryday/blob/master/Pictures/X.D.%20Camp/Solution.jpg?raw=true)

lock free queue & ABA problem

主线程的任务分解成两个线程进行：RenderThread和GameThread

生存周期越长越好，并发线程越多越好

UE4的生存时间比U3D长，并发线程多1个

### LWRP管线

最新，轻量管线，重点注意

![LWRP Batching](https://github.com/MilkyW/LearnUnityEveryday/blob/master/Pictures/X.D.%20Camp/LWRPBatching.jpg?raw=true)

是否开启Batch调用不同

开启后只需要初始化一次

### 内存模型

加速，快速申请与释放

#### ThreadsafeLinearAllocator

ThreadsafeLinearAllocatorHeader(64Bit)

- blockIndex

- size

- overhead

- magic

ThreadsafeLinearAllocatorBlock

- ptr

- usedSize

- allocationCount

特点：threadsafe, fast malloc/unfree, free all, reused bad/big waste, extra memory info

申请连续内存

#### PerThreadPageAllocator

AtomicPageAllocator

- CPU Cache Line(64 Bytes Align)

- m_ActivePages

PerThreadPageAllocator

- m_Start

- m_Offset

- m_CurrentPageSize

特点：malloc very fast, no free/free all, waste memory, little extra memory info

只申请不释放，结束时才会一次性释放

#### BucketAllocator

m_LargeBlocks

m_NewLargeBlockMutex

m_Buckets

m_BucketGranularityBits

Bucket

- availableBuckets

- m_LargeBlocks

- usedBucketsCount

- blockArray/block头+data

特点：fast malloc/free, lock less, extra memory info

#### DynamicHeapAllocator

([tlsf](https://github.com/mattconte/tlsf))Two-Level Segregated Fit Memory allocator

特点：

- O(1) cost for malloc, free, realloc, memalign

- Extremely low overhead per allocation (4 bytes)

- Low overhead per TLSF management of pools (~3kB)

- Low fragmentation

- Compiles to only a few kB of code and data

- Support for adding and removing memory pool regions on the fly

一边申请，一边释放

#### TLSAllocator

PlatformThreadSpecificValue

GetMemoryManager().ThreadInitialize(Thread::RunThreadWrapper)

StackAllocator

- Header/link list

- ThreadUnSafe

- overhead

特点：fast malloc/free?, thread safe

#### DualThreadAllocator

m_BucketAllocator

main thread/sub thread

- DynamicHeapAllocator

## Unity 图形渲染基础

[Unity3D渲染基础分享 - Prezi](https://prezi.com/view/v0bYcxT4jo5VmgIKqCpm/)

### Boat Attack Demo

Unity LWRP Demo: github.com/Verasl/BoatAttack

2018 unite:

PC FPS: 100左右, batching 1k

2019 unite: HWRP demo

### Pipeline

How to draw a frame?

**应用阶段**：场景数据，Culling，设置渲染状态

剔除：视锥剔除、遮挡剔除、静态剔除

渲染路径：如何遍历光和物体使他们发生作用

前向渲染：简单，两个循环嵌套，复杂度为O(光源数量\*物体数量)

延迟渲染：引入GBuffer，即Render Texture，很大的数组。先将所有物体画到GBuffer中再根据GBuffer绘制所有光源。将两个循环拆开，复杂度为O(光源数量\+物体数量)。对显卡要求高，需要支持OpenGL ES 3.0，去年在国内普及度为95~96%。每一帧要开五个GBuffer，对GPU带宽要求为五倍，所以耗手机电快。

将需要绘制的图元输入几何阶段

Batching

**几何阶段**：模型视图变换、顶点着色、投影、裁剪、屏幕映射

模型视图变换：Model->View(MVP)

Vertex Shading

两个投影矩阵：透视投影和正交投影

Clipping：裁剪，对完全不在里面的全部剔除，对一半在里面一半在外面的截断产生新的节点

**光栅化阶段**：三角形设置、三角形遍历、像素着色、合并

Triangle to Pixel

根据顶点的位置、深度、颜色对像素的颜色值进行线性插值

Pixel Shading

贴图对应坐标颜色覆盖或混合

Merging

片元->模版测试->深度测试->混合->颜色缓冲区

混合模式，Shader Blend Function

几何阶段处理大部分多边形操作和顶点操作后将数据传输至下一阶段

### SRP & LWRP

可编程渲染管线，与Unity内置Shader不兼容

需要支持OpenGL ES 3.1

LWRP Shading Models：基于物理的(Lit、Particles Lit)、风格化(Simple Lit、Simple Particles Lit)、静态光照(Baked Lit)

### Lit Shader

自发光，反射与折射

漫反射(固有色)、镜面反射，之间的比例

BaseColor、Roughness、Metallic

Screen Mapping、Direct Light、Base Map、Normal Map、Metallic Map、Receive Shadow

### Water System

1. Screen Mapping

2. Depth Base Color

3. Add Wave

4. Specular Map

5. Reflection 和Main Camera沿水面镜像绘制到1/4屏幕大小的贴图上或使用预先画好的反射探针(只能针对静态物体)

6. Detail Map

还有折射等等

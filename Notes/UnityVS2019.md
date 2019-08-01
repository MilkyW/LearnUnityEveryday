# Unity VS 2019开发环境搭建

**注意：如果有已经自行安装了这两个软件的同学，请检查安装的版本是否和要求一致。如有出入，请尽量按照教程重新安装一次。Unity版本是2019.1.8f1+离线文档，Visual Studio版本是2019+使用Unity的游戏开发。如果只是部分组件没有添加的话，可以打开Unity Hub和Visual Studio Installer进行修改。两个软件及组件都按照要求安装完毕后，请跳转至第三步：连接Unity和Visual Studio。**

## 安装Unity 2019
 
### 方法一：Unity Hub

 ![](https://github.com/MilkyW/LearnUnityEveryday/blob/master/Pictures/UnityVS2019/1-01.png?raw=true)

**1、下载Unity Hub**

**下载地址：** https://store.unity.com/cn/download

**2、等待传输完成并打开下载器。如果询问是否给予管理员权限/允许修改设备，选是。**

**3、点击我同意，（可以更改安装位置），点击安装。**

**4、等待安装完成，点击完成。**

**5、打开Unity Hub。**

 ![](https://github.com/MilkyW/LearnUnityEveryday/blob/master/Pictures/UnityVS2019/1-02.png?raw=true)

**6、点击管理许可证。**

 ![](https://github.com/MilkyW/LearnUnityEveryday/blob/master/Pictures/UnityVS2019/1-03.png?raw=true)

**7、点击登录。**

 ![](https://github.com/MilkyW/LearnUnityEveryday/blob/master/Pictures/UnityVS2019/1-04.png?raw=true)

**8、选择使用微信登陆，并用手机微信扫码，在手机上点击同意。**
 
 ![](https://github.com/MilkyW/LearnUnityEveryday/blob/master/Pictures/UnityVS2019/1-05.png?raw=true)

 ![](https://github.com/MilkyW/LearnUnityEveryday/blob/master/Pictures/UnityVS2019/1-06.png?raw=true)

**也可以用其他方式注册Unity账户，并在Unity Hub中登陆。**

 ![](https://github.com/MilkyW/LearnUnityEveryday/blob/master/Pictures/UnityVS2019/1-07.png?raw=true)

**9、登陆成功后，点击激活新许可证。**

![](https://github.com/MilkyW/LearnUnityEveryday/blob/master/Pictures/UnityVS2019/1-08.png?raw=true)

**10、选择Unity个人版，选择不以专业身份使用Unity，点击完成。等待许可证激活成功。**

![](https://github.com/MilkyW/LearnUnityEveryday/blob/master/Pictures/UnityVS2019/1-09.png?raw=true)

![](https://github.com/MilkyW/LearnUnityEveryday/blob/master/Pictures/UnityVS2019/1-10.png?raw=true)

**（可选）11、点击常规菜单，选择Unity安装路径并保存。**

![](https://github.com/MilkyW/LearnUnityEveryday/blob/master/Pictures/UnityVS2019/1-11.png?raw=true)

![](https://github.com/MilkyW/LearnUnityEveryday/blob/master/Pictures/UnityVS2019/1-12.png?raw=true)
 
**12、返回主界面。点击安装菜单。**

![](https://github.com/MilkyW/LearnUnityEveryday/blob/master/Pictures/UnityVS2019/1-13.png?raw=true)
 
![](https://github.com/MilkyW/LearnUnityEveryday/blob/master/Pictures/UnityVS2019/1-14.png?raw=true)

![](https://github.com/MilkyW/LearnUnityEveryday/blob/master/Pictures/UnityVS2019/1-15.png?raw=true)
 
**13、点击安装。选择Unity2019.1.8f1，点击下一步。选择Documentation和简体中文，点击完成。如果询问是否给予管理员权限/允许修改设备，选是。**

**14、等待安装完成。**

### 方法二：离线安装包

**注意：如果身边有人已经下载完了可以问ta拷一份，如果自己下载完了可以拷给还没下载完的人。**

**1、通过Torrent种子获取离线安装包。**

**下载地址：** https://download.unity3d.com/download_unity/7938dd008a75/Unity-2019.1.8f1.torrent?_ga=2.240658244.690207845.1562122811-320400500.1544670943

![](https://github.com/MilkyW/LearnUnityEveryday/blob/master/Pictures/UnityVS2019/1-16.png?raw=true)

![](https://github.com/MilkyW/LearnUnityEveryday/blob/master/Pictures/UnityVS2019/1-17.png?raw=true)

**2、使用迅雷等下载器打开种子。Windows用户仅选择Windows文件夹下的UnitySetup64.exe文件，Mac用户仅选择MacOSX文件夹下的Unity.pkg文件。**

**3、打开离线安装包并安装。推荐将安装目录选为非系统磁盘。在选择组件时推荐勾选Documentation和简体中文语言包，其他组件暂无必要勾选。**

**注：本方法仍推荐下载安装并配置Unity Hub，请完成方法一中的步骤1-11后再继续接下来的步骤。**

![](https://github.com/MilkyW/LearnUnityEveryday/blob/master/Pictures/UnityVS2019/1-18.png?raw=true)

**4、打开Unity Hub，选择安装菜单，点击添加已安装版本。**

![](https://github.com/MilkyW/LearnUnityEveryday/blob/master/Pictures/UnityVS2019/1-19.png?raw=true)

![](https://github.com/MilkyW/LearnUnityEveryday/blob/master/Pictures/UnityVS2019/1-20.png?raw=true)

**5、选择刚才安装目录下对应版本文件夹下Editor文件夹中的Unity.exe，点击Select Editor。安装菜单中应出现对应版本的Unity。**

## 安装Visual Studio 2019
 
![](https://github.com/MilkyW/LearnUnityEveryday/blob/master/Pictures/UnityVS2019/2-01.png?raw=true)
 
**1、下载社区（免费）版下载器。**

**下载地址：** https://visualstudio.microsoft.com/zh-hans/downloads/

**2、等待传输完成并打开下载器。如果询问是否给予管理员权限/允许修改设备，选是。**

**3、点击继续，并等待传输完成。**

![](https://github.com/MilkyW/LearnUnityEveryday/blob/master/Pictures/UnityVS2019/2-02.png?raw=true)
 
**4、勾选工作负载中移动与开发下的使用Unity的游戏开发，并且去除安装详细信息中的可选Unity编辑器。**

![](https://github.com/MilkyW/LearnUnityEveryday/blob/master/Pictures/UnityVS2019/2-03.png?raw=true)

![](https://github.com/MilkyW/LearnUnityEveryday/blob/master/Pictures/UnityVS2019/2-04.png?raw=true)
 
**（可选）5、选择更改位置，改到非系统磁盘。**

![](https://github.com/MilkyW/LearnUnityEveryday/blob/master/Pictures/UnityVS2019/2-05.png?raw=true)
 
**6、点击安装。**

 ![](https://github.com/MilkyW/LearnUnityEveryday/blob/master/Pictures/UnityVS2019/2-06.png?raw=true)

**（可选）7、选择安装到非系统磁盘并点击确定。**

**8、等待传输和安装完成。**

**9、等待自动打开，如无问题则安装完成。**

## 连接Unity和Visual Studio

 ![](https://github.com/MilkyW/LearnUnityEveryday/blob/master/Pictures/UnityVS2019/3-01.png?raw=true)

 ![](https://github.com/MilkyW/LearnUnityEveryday/blob/master/Pictures/UnityVS2019/3-02.png?raw=true)

**1、打开Unity Hub。点击新建右边的下三角，选择Unity2019.1.8f1版本。**

![](https://github.com/MilkyW/LearnUnityEveryday/blob/master/Pictures/UnityVS2019/3-03.png?raw=true)

**2、选择3D模版，可以更改项目名称和目录（整个路径不可有中文，不可有空格！否则将报错：Creating Project folder failed!），点击创建。等待创建完成，项目自动打开。**

![](https://github.com/MilkyW/LearnUnityEveryday/blob/master/Pictures/UnityVS2019/3-04.png?raw=true)

**3、选择菜单栏中的Edit，选择Preferences..**

![](https://github.com/MilkyW/LearnUnityEveryday/blob/master/Pictures/UnityVS2019/3-05.png?raw=true)

![](https://github.com/MilkyW/LearnUnityEveryday/blob/master/Pictures/UnityVS2019/3-06.png?raw=true)

**3、在弹出的窗口中，选择External Tools菜单，在External Script Editor中选择Visual Studio 2019(Community)。**

![](https://github.com/MilkyW/LearnUnityEveryday/blob/master/Pictures/UnityVS2019/3-07.png?raw=true)

**（可选）4、如没有这个选项，选择Browse..，选中Visual Studio安装目录下对应版本文件夹下的Common7文件夹下的IDE文件夹中的devenv.exe，点击打开。External Script Editor可选列表中应出现Visual Studio 2019。**

![](https://github.com/MilkyW/LearnUnityEveryday/blob/master/Pictures/UnityVS2019/3-08.png?raw=true)

**5、在Project窗口中右键，选择Create，选择C# Script，随意命名后双击打开。**

![](https://github.com/MilkyW/LearnUnityEveryday/blob/master/Pictures/UnityVS2019/3-09.png?raw=true)

![](https://github.com/MilkyW/LearnUnityEveryday/blob/master/Pictures/UnityVS2019/3-10.png?raw=true)

**6、等待Visual Studio 2019自动启动完毕。点击上方附加到Unity，等待左下角出现就绪字样。至此，确认配置完成。**
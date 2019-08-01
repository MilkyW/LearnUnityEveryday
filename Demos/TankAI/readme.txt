# AI结构
	AI脚本是一个继承自BaseControl类的Component，挂在一个GameObject下面供程序调用
	使用方式:
		在Managers/GameSetting下面有一个GameSettings的简单设置:
		PlayerControl : 手动操作的控制器，不用改动
		PlayerAI : 我方Player的控制器
		BotAI : 测试用Bot的控制器

# BasePlayer
	坦克的基类，对于测试用的Bot是PlayerBot，对于玩家是Player。
	BasePlayer主要提供以下方法:

	myName 	  : 玩家名字
	teamIndex : 队伍ID
	health 	  : 当前hp
	maxhealth : 最大hp
	shield    : 护盾值(抵挡伤害次数)
	ammo 	  : 特殊弹药数
	currentBullet : 当前的弹药类型(0:普通 1:重伤 2:反弹)
	上述字段由host同步，不要本地修改。

	Position  : 坦克的位置
	Velocity  : 坦克的移动速度
	IsAlive   : 是死是活
	bShootable : 当前是否可以射击

	SimpleMove(direction) : 向某方向移动，参数是代表方向的Vector2(x,z);
	MoveTo(position) 	  : 移动到地图上的某个点
	RotateTurret(dx,dz)	  : 转动炮台到指定方向，dx和dz是自身到目标位置的位置差。
	Shoot() 			  : 向当前炮台方向射击
	AimAndShoot(targetPosition) : 如果能打到的话，瞄准并射击地图上的某个目标点。

	坦克的移动速率为常量 8米/s
	坦克的攻击间隔为常量 0.75秒

	禁止直接修改Transform。

# BaseControl
	坦克控制器的Component。
	OnInit    : 控制器的初始化操作
	OnRun	  : 在坦克创建/重生时调用，OnInit之后 
	OnStop    : 坦克死亡时候调用，停止该AI
	OnFixedUpdate : BasePlayer的FixedUpdate调用
	OnUpdate  : BasePlayer的Update调用

	tankPlayer : 该AI附加到的坦克对象BasePlayer

# Bullet
	speed 		 : 子弹的移动速率
	damage  	 : 子弹的伤害
	Position     : 子弹的位置
	Velocity     : 子弹的速度

# Powerup
	强化道具，包括特殊子弹/血包/护盾..
	PowerupBullet	特殊子弹
		ammount  : 特殊子弹数量
		bulletIndex: 特殊子弹类型   1为重伤弹 2为反弹弹

	PowerupHealth   血包
		ammount  : 恢复血量

	PowerupShield   护盾
		ammount  : 护盾抵挡次数

# Tags & Layers
	坦克都加上了 Player Tag, Player Layer
	子弹都加上了 Bullet Tag, Bullet Layer
	道具都加上了 Powerup Tag, Powerup Layer 
	方便查找。








### Using the Unity Interface Tutorial
**Scene View**
**Tool change:** QWERT

**Hand tool:** (Alt + Ctrl/Command) + Drag

**Fly-through mode:** (Right Mouse Button) + WASD(move)/QE(change height): (holding: continue to accelerate)

**Rotate when transform tool selected:** (Alt) + (Left Mouse Button)Drag

**Zooms when transform tool selected:** (Alt) + (Right Mouse Button)Drag

**Focus on the frame selected:** selected + F

**Searching for type(tag/Component):** “t:” + …

**Inspector Debug Mode**

### Roll-a-ball Tutorial
**Principle:**
Test early and test often.
Use <u>contrast color</u> and <u>motion</u> to attract the player.

| Static Collider | Dynamic Collider |
| --- | --- |
| Collider only | Collider and Rigidbody |
| shouldn’t move | can move |
| like walls and floors | like the player |

| Standard Rigidbody | Kinematic Rigidbody |
| --- | --- |
| is moved using physics forces | will not react to any physics forces |
| shouldn’t be animated and moved by its Transform | can be animated and moved by its Transform <u>only</u> |
|  | like elevators, moving platforms and objects with triggers |

![Static and Dynamic Physics](https://github.com/MilkyW/LearnUnityEveryday/blob/master/Pictures/Static%20and%20Dynamic%20Physics.png?raw=true)

**Static & Dynamic:** Collider and Rigidbody.

Static Collider cache is recalculated every time the Transform of any of them is changed.

A kinematic Rigidbody will not react to any physics forces, and can be animated and moved by its Transform, like elevators and moving platforms to objects with triggers.

Static Colliders shouldn’t move, like walls and floors. Dynamic Colliders can move and have a Rigidbody attached.

Standard Rigidbodies are moved using physics forces. Kinematic Rigidbodies are moved using their Transform.

**API:**

**LateUpdate()** runs every frame after all the Update()s, used for <u>follow cameras</u>, <u>procedural animation</u> and <u>gathering last known states</u>.

**@Update Order**

**\*Time.deltaTime** smooth and frame rate independent. for Update() and LateUpdate(), but needless for FixedUpdate().

gameObject.**CompareTag(tag)** is more efficient than gameObject.tag == tag. Tag is case-sensitive.

### 2D UFO Tutorial

**Sorting layer:** may be modified at importing(change project settings), rendering order from top to bottom(sorting layer at the bottom is rendered the last and thus rendered on the top of others)

**2D & 3D physical systems:** comparable and compatible to each other(can be both in the same scene), but will not interact with each other

**Collider:** The physics system thinks a single solid object shouldn't be in the middle of another single solid object. The physics engine's solution is to push the two objects apart by adding forces to any attached rigidbody.
It's fairly common for colliders to overlap each other when laying out levels using static non-moving objects.

### Beginner Gameplay Scripting
**Do-while loop:** ended by ;

**Update():** Called just before every frame; Used for moving non-physics objects, simple timers, receiving input; update interval times vary

**FixedUpdate():** Called just before every physics calculation; Fixed update intervals are consistent; used for adjusting physics(Rigidbody) objects

gameObject.**activeSelf/activeInHierarchy:** is (not) related to the activeness of the ancestor

transform.**Translate/Rotate:** should be used for static collider/dynamic collider with kinematic rigidbody

**Vector3.forward/up:** is local

**transform.LookAt(transform):** changes local forward direction

Mathf/Vector3/Color.**Lerp**(from, to, percentage)

**SmoothDamp:** used to smooth a value over time

**GetButtion**>**GetKey**

**GetAxis:** (sensitivity + gravity)/GetAxisRaw(1/0, not smooth), snap(up + down = 0)

**GetComponent** is expensive: don’t use it in update()

**Datatypes:** built-in(number, Vector) & struct: <u>value</u>, class: <u>reference</u>

**Invoke**(string functionName, float delay)

InvokeRepeating(string functionName, float delay, float interval): function should return void and has no parameters

CancelInvoke(optional string functionName): cancel all the invokes if no parameter

**enum:** change type by : type, ended by ;

### Intermediate Gameplay Scripting

```cs
private int experience;
public int Experience {
    get { return experience; }
    set { experience = value; }
} 

public int Level {
    get { return experience / 1000; }
    set { experience = value * 1000; }
} 

public int Health{ get; set; }
```
**Proporties:** explicit defined get and set (also shorthand syntax) to encapsulate fields and gain control of access out side the class, used for security check/read-only/write-only

```cs
Input.GetButtonDown(“Fire1”);
Vector3.zero;
public class Enemy { 
    public static int enemyCount = 0;
    public Enemy() { enemyCount++; }
}
```

**Statics:** variable(shared to the class), method(shared to the class, only access to static variables), class(cannot be instantiated, thus only has static variables and static methods, such as Input), can be accessed without instantiating one

**Overload:** signature = functionName + parameterList, signature is unique in the scope. Exact Match->Least Conversion Match->Error(no possible matches or least conversion match not unique)

**Generics:** to add constrains: where T : …(, …, …) class(reference type)/struct(value type), new() (has a public constructor with no parameters), ClassName(is that class or derived from that class), InterfaceName(has implemented that interface) to implement data structure. Applies to classes, interfaces and methods.

**Access modifier:** public, protected, private

**Call specific Parent Constructor:** public Child(parameters...): base(arguments…)

**Up-casting:** threat children as if they were parents(can only use variables and methods in the parent class), except for the virtual function, which calls the most overridden version.

**Down-casting:** `((Child)myClass).childMethod()` or `Child myChild = (Child)myClass` to create a reference

**Overriding:** virtual + override(to suppress warning), when up-casting, the child version of the method will be called, use ‘base’ to call the parent version in the child version

**Member hiding:** use ’new’ to do the opposite of overriding, when up-casting, the parent version of method will be called

```cs
// declared outside any class
public interface IAbcable<T> { 
    returnType functionName(argList);
} 

class ABC: MonoBehaviuor, IAbcable<typeName> {
    public returnType functionName(argList){ … }
}
```

**Interfaces:**  Contract of class on functionality(public variables and methods), used to defined <u>common functionality</u> across classes unrelated to each other. Not classes, cannot have instances. Declared in interface, implemented in classes.

ClassA implements InterfaceB, must public declare all of the methods, properties, events and indexers or an error will occur.

**List:** in order to use `Sort()`, implement IComparable<T> public int CompareTo(TypeName other)

```cs
public static class ExtensionMethods { 
    public static void ResetTransformation(this Transform trans) {     
        trans.position = Vector3.zero;
        trans.localRotation = Quaternion.identity;
        trans.localScale = new Vector3(1, 1, 1);
        }
}

transform.ResetTransformation();
```

**Extension Methods:** adding functionality to a type without having to create a derived type or changing the original type(don’t have access to built-in classes like Transform in Unity)

must be placed in a non-generic static class(create a class specifically to contain them), used like instance method, static themselves. use ‘this’ before the typeName of the first parameter(the type being extended, will be the calling object implicitly).

**Dictionary:** using System.Collections.Generic; TryGetValue(keyType key) safer(direct access will throw exception when not exists) but slightly slower. 

```cs
IEnumerator FunctionName(argList){
    // some code here
    yield return ...;
    // some code here
}

StartCoroutine(FunctionName, argList);
// or
StartCoroutine(FunctionName(argList));

StopCoroutine(FunctionName);
```

**Coroutines:** create a movement without using update or creating a timer, more <u>code efficiency</u>(not pulling for values every frame).

**Quaternions:** more difficult to understand than Euler Angle but not subjected to the gimbal lock(prevent incremental rotations), XYZW are <u>interdependent</u> and shouldn't be adjusted individually, use the built-in method instead

![Lerp and Slerp](https://github.com/MilkyW/LearnUnityEveryday/blob/master/Pictures/Lerp%20and%20Slerp.png?raw=true)

**Lurp:** linear interpolation, interpolate evenly

**Slurp:** spherical interpolation, interpolate on a curve(start and stop slower, faster in the middle)

```cs
[Attribute(argList)]
```

**Attributes:** to attach information to variable, method and class, put above or just before

`[Range(min, max)] // variable declaration`

`[ExecuteInEditMode]` not in play mode, will modify the scene permanently and cannot revert!

```cs
delegate returnType DelegateName(argList);
DelegateName delegateName;
delegateName = FunctionName;
delegateName(argList);
```

**Delegates:** create robust and complex behaviours

muti-casting:  `delegateName += FunctionName;` execute several functions in a single call(stack functionality) use `-=` to remove. check `!= null` before calling

shouldn't count on its order.

```cs
public delegate void ClickAction();
public static event ClickAction OnClicked;
```

**Events:** specialized delegates, used to alert other classes that something has happened(robust and flexible broadcast system).Subscribe through `+=`, unsubscribe through `-=`. Rule: always <u>pair</u> subscribe with unsubscribe. Use event instead of public delegate for <u>security</u>(other class can only subscribe and unsubscribe, but cannot invoke or override) when to use: want to create a dynamic method system involves more than one class

**UnityAction:** to add +=/delete -= methods. can use lambda for convenience.

**UnityEvent:** event.AddListener(action)/RemoveListener(action) to subscribe/unsubscribe. event.Invoke(argList) to trigger. generic is abstract and should be specific. event's argList should match with action's. can use lambda for convenience.

[MessageSystem](https://github.com/KEEMU/MessageSystem)

### AR Foundation
Unity 2018
Package Manager

### StateMachineBehaviour

OnStateEnter/Update/Exit->Animation Clip in Animator

### UGUI
**CanvasRenderer:** 并不是Renderer的子类。

**Event Trigger in Canvas:** No <u>canvas renderer</u> in the area, <u>UGUI block</u>: Transition->Pointer ScrollView Content->Drag

### iOS Develope
![Apple Develop](https://github.com/MilkyW/LearnUnityEveryday/blob/master/Pictures/Apple%20Develop.png?raw=true)

**Apple Developer account:**

[Building iOS Application](https://confluence.hq.unity3d.com/display/QA/Building+iOS+Application)

**macOS, Xcode and iOS SDK:**
**Xcode 10.1:** highest for macOS 10.13.x

[Xcode_10.1.xip](https://drive.google.com/file/d/1L0A81DdYBrOUYlG1Xi1C49_uSvSG_71l/view?usp=sharing)

[Xcode_10.1.xip](https://download.developer.apple.com/Developer_Tools/Xcode_10.1/Xcode_10.1.xip)

**iOS SDK:** copy from higher versions of Xcode

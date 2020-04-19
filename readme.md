# RayTracingWithU3DEditor

# Whitted-Style RayTracing

read this first https://raytracing.github.io/books/RayTracingInOneWeekend.html

PDF https://www.realtimerendering.com/raytracing/Ray%20Tracing%20in%20a%20Weekend.pdf


Codes Follow this article ：https://zhuanlan.zhihu.com/p/36238483

# Some Basic idea.

Ray tracing in one weekend 这里说明的是 Whitted-Style 风格的光线追踪

默认模拟的是有透视的场景.

lowLeftCorner 是模拟的视椎体的左下角
所以根据 horizontal  vertical  得到的场景范围是

(-2, 1, -1)      (2, 1, -1)

(-2, -1, -1)     (2, -1, -1)

另外规定在这个幕布上有 400 * 200 个像素点

传统的右手坐标系

摄像头在 (0 0 0) 的位置   屏幕(也就是远面) 在 z 轴 -1 的位置

根据右手坐标系传统, 摄像头默认面对的是 z 的负方向.

在这篇说明中, 光线都是从center(0,0,0) 出发, 然后打在屏幕上

在本篇说明中, 光线就是从摄像头出发的, 你可以理解为整个图都是由相机闪光灯拍摄的结果.

光线射向整个场景的每一个角落(也就是射向400 * 200 中每一个像素点), 然后计算颜色

PS:
本篇没有涉及到近面


PS: 角度和弧度

1°=π/180   1rad=180°/π

PS:
Math.Tan(xxx) 这里xxx 都是弧度, 要计算角度的话 需要 Math.Tan(π/180 * YOUR_ANGLE)


# 测试一个简单的球体

球心在屏幕的位置 (0, 0, -1)  GetColorForTestSphere

由于摄像头和屏幕距离很近(屏幕z是-1  摄像机在0), 所以视椎体是一个很广角的感觉, 侧边的物体会有很大程度上的畸变


例如当你在 GetColorForTestSphere 把球坐标改为 (1, 0, -1) 时, 得到结果是

![image](https://raw.githubusercontent.com/lumixraku/RayTracingWithU3DEditor/master/畸变.png)

不过要是稍微把球放远一点(1, 0, -2), 这个情况就会好很多.

![image](https://raw.githubusercontent.com/lumixraku/RayTracingWithU3DEditor/master/畸变1.png)

当然, 也可以移动相机的位置, 部分场景中我摄像机放在(0,0,3)的位置


## 关于HitRecord

- t 是碰撞时光线的长度
- p 是光线和球碰撞的点, 也就是交汇点
- normal 是交汇点的法线(通过 p - center 得到)
- material 不同的材料会对碰撞的光线有不同的行为

# 抗锯齿


锯齿的来源是因为场景的定义在三维空间中是连续的，而最终显示的像素则是一个离散的二维数组。所以判断一个点到底没有被某个像素覆盖的时候单纯是一个“有”或者“没有"问题，丢失了连续性的信息，导致锯齿。

更深层的原因其实就是采样率不够(真实物体时连续, 但是采样是离散的)

本篇使用的方法应该是SSAA 超采样

使用了随机算法来采样, 详情 GetColorForTestAntialiasing.

针对每个像素点, 光源(也就是摄像机所在位置)都再额外多发出100条光线去采样计算, 这些光线在原有的光线方向上略微有些发散, 最后取得到的颜色的均值.


# 散射

本篇仅说明 diffuse

详情 CreateColorForTestDiffusing

GetColorForTestDiffusing 会递归调用

同样针对每个像素点, 都再发射一些光线计算.

record 是光线和球碰撞交汇的记录. 碰撞点都再次发出光线(衰减50%), 这些光线方向随机, 再次参与光线碰撞计算.

PS (漫反射场景有两个球, 底部有一个很大的球, 存在光线的弹射)

如果去掉底部那个球, 呈现的结果会是一个很光滑的蓝色球.

![image](https://raw.githubusercontent.com/lumixraku/RayTracingWithU3DEditor/master/漫反射.png)
![image](https://raw.githubusercontent.com/lumixraku/RayTracingWithU3DEditor/master/漫反射2.png)


## 镜面
详情 CreateColorForTestMetal

GetColorForTestMetal 中对每一个光线打到的碰撞点有scatter 函数

这个HitRecord 会调用不同材质的散射函数.

- 对于漫反射, 得到的光的方向时随机的, 向四面八方
- 对于金属类型, 根据设置的fuzz 加上一定程度的漫反射(也就是常说的 glossy 的概念),
  剩下的最主要的还是反射(需要知道入射光线和法线才好做反射),

- 电解质(玻璃) 需要反射和折射

# 测试相机FOV和位置角度

![image](https://raw.githubusercontent.com/lumixraku/RayTracingWithU3DEditor/master/FOV.png)


详情 GetColorForTestCamera  L552

这里使用的是标准Camera, lookFrom 和 lookAt 就定义了摄像头的朝向.

而Vup 就相机围绕着这个朝向的旋转的角度(就相当于拍摄的时候相机已经对准了物体, 但是需要决定是竖着拿还是横着拿相机). 默认是Up, 不旋转相机.

## FOV
vfov  视角(angle of view)与成像范围(angle of coverage)是不同的

表现上是变焦, 值越小, 放大倍数越大.

FOV 和变焦的关系
https://blog.csdn.net/l773575310/article/details/78401864

zoom = 1/(tan(fov/2))

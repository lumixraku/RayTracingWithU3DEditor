using System.Collections;
using UnityEditor;
using System.IO;
using UnityEngine;
using System.Collections.Generic;

namespace SimpleRT
{

    public class RayTracingDemo : EditorWindow
    {
        [MenuItem("Noobdawn/光线追踪渲染器")]
        public static void OnClick()
        {
            RayTracingDemo window = GetWindow<RayTracingDemo>();
            window.Show();
        }

        void OnGUI()
        {
            if (GUILayout.Button("测试图片"))
            {
                CreatePng(WIDTH, HEIGHT, CreateColorForTestPNG(WIDTH, HEIGHT));
            }
            if (GUILayout.Button("测试射线"))
            {
                CreatePng(WIDTH, HEIGHT, CreateColorForTestRay(WIDTH, HEIGHT));
            }
            if (GUILayout.Button("测试简单球体"))
            {
                CreatePng(WIDTH, HEIGHT, CreateColorForTestSphere(WIDTH, HEIGHT));
            }
            if (GUILayout.Button("测试球体法线"))
            {
                CreatePng(WIDTH, HEIGHT, CreateColorForTestNormal(WIDTH, HEIGHT));
            }
            if (GUILayout.Button("抽象碰撞信息"))
            {
                CreatePng(WIDTH, HEIGHT, CreateColorForTestHitRecord(WIDTH, HEIGHT));
            }
            if (GUILayout.Button("测试反锯齿"))
            {
                CreatePng(WIDTH, HEIGHT, CreateColorForTestAntialiasing(WIDTH, HEIGHT));
            }
            if (GUILayout.Button("测试散射模型"))
            {
                CreatePng(WIDTH, HEIGHT, CreateColorForTestDiffusing(WIDTH, HEIGHT));
            }
            if (GUILayout.Button("测试镜面模型"))
            {
                CreatePng(WIDTH * 2, HEIGHT, CreateColorForTestMetal(WIDTH * 2, HEIGHT));
            }
            if (GUILayout.Button("测试透明模型"))
            {
                CreatePng(WIDTH, HEIGHT, CreateColorForTestDielectric(WIDTH, HEIGHT));
            }
            if (GUILayout.Button("测试相机FOV和位置角度"))
            {
                CreatePng(WIDTH, HEIGHT, CreateColorForTestCamera(WIDTH, HEIGHT));
            }
            if (GUILayout.Button("测试景深"))
            {
                CreatePng(WIDTH, HEIGHT, CreateColorForTestDefocus(WIDTH, HEIGHT));
            }
            if (GUILayout.Button("测试超大随机场景"))
            {
                CreatePng(WIDTH, HEIGHT, CreateColorForTestRandomBalls(WIDTH, HEIGHT));
            }

        }
        #region 参数设定
        const string IMG_PATH = @"/Users/lilin/unity/raytrace/1.png";
        const int WIDTH = 400;
        const int HEIGHT = 200;
        const int SAMPLE = 100;
        const float SAMPLE_WEIGHT = 0.01f;
        const int MAX_SCATTER_TIME = 50;
        #endregion
        #region 第一版（测试输出图片）
        Color[] CreateColorForTestPNG(int width, int height)
        {
            int l = width * height;
            Color[] colors = new Color[l];
            for (int j = height - 1; j >= 0; j--)
                for (int i = 0; i < width; i++)
                {
                    colors[i + j * width] = new Color(
                        i / (float)width,
                        j / (float)height,
                        0.2f);
                }
            return colors;
        }
        #endregion
        #region 第二版（测试射线、简单的摄像机和背景）
        Color GetColorForTestRay(Ray ray)
        {
            float t = 0.5f * ray.normalDirection.y + 1f;
            return (1 - t) * new Color(1, 1, 1) + t * new Color(0.5f, 0.7f, 1);
        }

        Color[] CreateColorForTestRay(int width, int height)
        {
            //视锥体的左下角、长宽和起始扫射点设定
            Vector3 lowLeftCorner = new Vector3(-2, -1, -1);
            Vector3 horizontal = new Vector3(4, 0, 0);
            Vector3 vertical = new Vector3(0, 2, 0);
            Vector3 original = new Vector3(0, 0, 0);
            int l = width * height;
            Color[] colors = new Color[l];
            for (int j = height - 1; j >= 0; j--)
                for (int i = 0; i < width; i++)
                {
                    Ray r = new Ray(original, lowLeftCorner + horizontal * i / (float)width + vertical * j / (float)height);
                    colors[i + j * width] = GetColorForTestRay(r);
                }
            return colors;
        }
        #endregion
        #region 第三版（测试一个简单的球体）
        bool isHitSphereForTestSphere(Vector3 center, float radius, Ray ray)
        {
            var oc = ray.original - center;
            float a = Vector3.Dot(ray.direction, ray.direction);
            float b = 2f * Vector3.Dot(oc, ray.direction);
            float c = Vector3.Dot(oc, oc) - radius * radius;
            //实际上是判断这个方程有没有根，如果有2个根就是击中
            float discriminant = b * b - 4 * a * c;
            return (discriminant > 0);
        }

        Color GetColorForTestSphere(Ray ray)
        {
            if (isHitSphereForTestSphere(new Vector3(0, 0, -1), 0.5f, ray))
                return new Color(1, 0, 0);
            float t = 0.5f * ray.normalDirection.y + 1f;
            // 渐变背景色
            return (1 - t) * new Color(1, 1, 1) + t * new Color(0.5f, 0.7f, 1);
        }

        Color[] CreateColorForTestSphere(int width, int height)
        {
            //视锥体的左下角、长宽和起始扫射点设定
            Vector3 lowLeftCorner = new Vector3(-2, -1, -1);
            Vector3 horizontal = new Vector3(4, 0, 0);
            Vector3 vertical = new Vector3(0, 2, 0);
            Vector3 original = new Vector3(0, 0, 0); //球心处于整个视锥体中心位置
            int l = width * height;
            Color[] colors = new Color[l];
            for (int j = height - 1; j >= 0; j--)
                for (int i = 0; i < width; i++)
                {
                    // 光线从球心出发 射向camera镜头的的各个区域
                    Ray r = new Ray(original, lowLeftCorner + horizontal * i / (float)width + vertical * j / (float)height);
                    colors[i + j * width] = GetColorForTestSphere(r);
                }
            return colors;
        }
        #endregion
        #region 第四版（测试球体的表面法线）
        float HitSphereForTestNormal(Vector3 center, float radius, Ray ray)
        {
            var oc = ray.original - center;
            float a = Vector3.Dot(ray.direction, ray.direction);
            float b = 2f * Vector3.Dot(oc, ray.direction);
            float c = Vector3.Dot(oc, oc) - radius * radius;
            //实际上是判断这个方程有没有根，如果有2个根就是击中
            float discriminant = b * b - 4 * a * c;
            if (discriminant < 0)
            {
                return -1;
            }
            else
            {
                //返回距离最近的那个根
                return (-b - Mathf.Sqrt(discriminant)) / (2f * a);
            }
        }

        Color GetColorForTestNormal(Ray ray)
        {
            float t = HitSphereForTestNormal(new Vector3(0, 0, -1), 0.5f, ray);
            if (t > 0)
            {
                Vector3 normal = Vector3.Normalize(ray.GetPoint(t) - new Vector3(0, 0, -1));
                return 0.5f * new Color(normal.x + 1, normal.y + 1, normal.z + 1, 2f);
            }
            t = 0.5f * ray.normalDirection.y + 1f;
            return (1 - t) * new Color(1, 1, 1) + t * new Color(0.5f, 0.7f, 1);
        }

        Color[] CreateColorForTestNormal(int width, int height)
        {
            //视锥体的左下角、长宽和起始扫射点设定
            Vector3 lowLeftCorner = new Vector3(-2, -1, -1);
            Vector3 horizontal = new Vector3(4, 0, 0);
            Vector3 vertical = new Vector3(0, 2, 0);
            Vector3 original = new Vector3(0, 0, 0);
            int l = width * height;
            Color[] colors = new Color[l];
            for (int j = height - 1; j >= 0; j--)
                for (int i = 0; i < width; i++)
                {
                    Ray r = new Ray(original, lowLeftCorner + horizontal * i / (float)width + vertical * j / (float)height);
                    colors[i + j * width] = GetColorForTestNormal(r);
                }
            return colors;
        }
        #endregion
        #region 第五版（测试Hit的抽象）(抽象碰撞信息)
        Color GetColorForTestHitRecord(Ray ray, HitableList hitableList)
        {
            HitRecord record = new HitRecord();
            if (hitableList.Hit(ray, 0f, float.MaxValue, ref record))
            {
                // 没有直接返回 record.normal.xyz 是因为默认颜色有点暗
                return 0.5f * new Color(record.normal.x + 1, record.normal.y + 1, record.normal.z + 1, 2f);
            }
            float t = 0.5f * ray.normalDirection.y + 1f;

            // 控制浅蓝的背景色  
            return (1 - t) * new Color(1, 1, 1) + t * new Color(0.5f, 0.7f, 1);
        }

        Color[] CreateColorForTestHitRecord(int width, int height)
        {
            //视锥体的左下角、长宽和起始扫射点设定
            Vector3 lowLeftCorner = new Vector3(-2, -1, -1);
            Vector3 horizontal = new Vector3(4, 0, 0);
            Vector3 vertical = new Vector3(0, 2, 0);
            Vector3 original = new Vector3(0, 0, 0);
            int l = width * height;
            HitableList hitableList = new HitableList();
            hitableList.list.Add(new SimpleSphere(new Vector3(0, 0, -1), 0.5f));  //第一个参数是球心  第2个参数是半径
            hitableList.list.Add(new SimpleSphere(new Vector3(0, -100.5f, -1), 100f));
            Color[] colors = new Color[l];

            float recip_width = 1f / width;  // 1/400
            float recip_height = 1f / height;  // 1/200

            for (int j = height - 1; j >= 0; j--)
                for (int i = 0; i < width; i++)
                {
                    Ray r = new Ray(original, lowLeftCorner + horizontal * i / recip_width + vertical * j / recip_height);
                    colors[i + j * width] = GetColorForTestHitRecord(r, hitableList);
                }
            return colors;
        }
        #endregion
        #region 第六版（测试抗锯齿）
        Color GetColorForTestAntialiasing(Ray ray, HitableList hitableList)
        {
            HitRecord record = new HitRecord();

            // 实际上调用的是 SimpleSphere 的 Hit
            if (hitableList.Hit(ray, 0f, float.MaxValue, ref record))
            {

                // 这里+1 都是为了更好的视觉效果
                return 0.5f * new Color(record.normal.x + 1, record.normal.y + 1, record.normal.z + 1, 2f);
            }
            float t = 0.5f * ray.normalDirection.y + 1f;

            // 控制浅蓝的背景色  
            return (1 - t) * new Color(1, 1, 1) + t * new Color(0.5f, 0.7f, 1);
        }


        Color[] CreateColorForTestAntialiasing(int width, int height)
        {
            // width height 是像素点单位400  200
            // 视锥体的左下角、长宽和起始扫射点设定

            Vector3 lowLeftCorner = new Vector3(-2, -1, -1);
            Vector3 horizontal = new Vector3(4, 0, 0);
            Vector3 vertical = new Vector3(0, 2, 0);
            Vector3 original = new Vector3(0, 0, 0);
            int l = width * height;
            HitableList hitableList = new HitableList();

            //这样的球在偏右的位置
            //hitableList.list.Add(new SimpleSphere(new Vector3(1, 0, -1), 0.5f));
            hitableList.list.Add(new SimpleSphere(new Vector3(0, 0, -1), 0.5f));
            hitableList.list.Add(new SimpleSphere(new Vector3(0, -100.5f, -1), 100f));
            Color[] colors = new Color[l];


            // 摄像头在0 0 0的位置
            // 屏幕在 z轴 -1 的位置
            // 传统的右手坐标系
            SimpleCamera camera = new SimpleCamera(original + new Vector3(0, 0, 3), lowLeftCorner, horizontal, vertical);
            float recip_width = 1f / width;  // 1/400
            float recip_height = 1f / height;  // 1/200
            for (int j = height - 1; j >= 0; j--)
                for (int i = 0; i < width; i++)
                {
                    Color color = new Color(0, 0, 0);

                    // 每个像素点都再发射100条光线去采样计算
                    for (int s = 0; s < SAMPLE / 10; s++)
                    {
                        // _M.R() 生成0~1随机数
                        // 创建的ray  出发点在球心  也就是original  //注意 ！！ CreateRay包含了vertical 和 horizontal 的尺寸计算
                        Ray r = camera.CreateRay((i + _M.R()) * recip_width, (j + _M.R()) * recip_height);
                        //下面关闭抗锯齿的情形，就和第5版一样的
                        //Ray r = camera.CreateRay(i * recip_width, j * recip_height);
                        color += GetColorForTestAntialiasing(r, hitableList);
                    }
                    color *= SAMPLE_WEIGHT * 10;
                    color.a = 1f;
                    colors[i + j * width] = color;
                }
            return colors;
        }
        #endregion
        #region 第七版（测试Diffuse）（散射模型）
        //此处用于取得无序的反射方向，并用于模拟散射模型
        Vector3 GetRandomPointInUnitSphereForTestDiffusing()
        {
            // Vector3(_M.R(), _M.R(), _M.R()) 这个向量的每一个分量变化范围是 0~1
            // 现在要得到 [-1, 1]
            Vector3 p = 2f * new Vector3(_M.R(), _M.R(), _M.R()) - Vector3.one;
            p = p.normalized;
            //Vector3 p = Vector3.zero;
            //do
            //{
            //    p = 2f * new Vector3(_M.R(), _M.R(), _M.R()) - Vector3.one;
            //}
            //while (p.sqrMagnitude > 1f); // sqrMagnitude  返回向量的长度
            return p;
        }
        // https://www.guokr.com/question/569971/ 
        // 光线追踪 如果遇到漫反射面的话一般是需要产生非常多的次级射线往下递归才能达到比较好的效果（否则噪点比较明显）
        Color GetColorForTestDiffusing(Ray ray, HitableList hitableList)
        {
            HitRecord record = new HitRecord();
            // 这里对应的是 simpleSphere 的 Hit 方法
            if (hitableList.Hit(ray, 0.0001f, float.MaxValue, ref record))
            {
                Vector3 target = record.p + record.normal + GetRandomPointInUnitSphereForTestDiffusing();
                //此处假定有50%的光被吸收，剩下的则从入射点开始取随机方向再次发射一条射线
                //return 0.5f * GetColorForTestDiffusing(new Ray(record.p, target - record.p), hitableList);

                // 即便稳重说的是使用随机的光线 但是即使是漫反射 也还是和物体本身的法线方向相关的  //所以这里还需要考虑到法线
                return 0.5f * GetColorForTestDiffusing(new Ray(record.p, record.normal + GetRandomPointInUnitSphereForTestDiffusing() * 0.5f), hitableList);
            }
            float t = 0.5f * ray.normalDirection.y + 1f;
            return (1 - t) * new Color(1, 1, 1) + t * new Color(0.5f, 0.7f, 1);
        }

        Color[] CreateColorForTestDiffusing(int width, int height)
        {
            //视锥体的左下角、长宽和起始扫射点设定
            Vector3 lowLeftCorner = new Vector3(-2, -1, -1);
            Vector3 horizontal = new Vector3(4, 0, 0);
            Vector3 vertical = new Vector3(0, 2, 0);
            Vector3 original = new Vector3(0, 0, 0);
            int l = width * height;
            HitableList hitableList = new HitableList();

            // 创建了两个球体
            hitableList.list.Add(new SimpleSphere(new Vector3(0, 0, -1), 0.5f));
            hitableList.list.Add(new SimpleSphere(new Vector3(0, -100.5f, -1), 100f));
            Color[] colors = new Color[l];
            SimpleCamera camera = new SimpleCamera(original, lowLeftCorner, horizontal, vertical);
            float recip_width = 1f / width;
            float recip_height = 1f / height;
            for (int j = height - 1; j >= 0; j--)
                for (int i = 0; i < width; i++)
                {
                    Color color = new Color(0, 0, 0);
                    for (int s = 0; s < SAMPLE; s++)
                    {
                        Ray r = camera.CreateRay((i + _M.R()) * recip_width, (j + _M.R()) * recip_height);
                        color += GetColorForTestDiffusing(r, hitableList);
                    }
                    // 刚才上面color 加了 SAMPLE 次光线  导致球体本身很亮
                    color *= SAMPLE_WEIGHT;
                    //为了使球体看起来更亮，改变gamma值
                    color = new Color(Mathf.Sqrt(color.r), Mathf.Sqrt(color.g), Mathf.Sqrt(color.b), 1f);
                    // color.a = 1f;
                    colors[i + j * width] = color;
                }
            return colors;
        }
        #endregion
        #region 第八版（测试镜面）
        Color GetColorForTestMetal(Ray ray, HitableList hitableList, int depth)
        {
            HitRecord record = new HitRecord();

            // 当光线撞击在球上的时候  // 这里包括了法线毛绒球和两个金属球
            if (hitableList.Hit(ray, 0.0001f, float.MaxValue, ref record))
            {
                Ray r = new Ray(Vector3.zero, Vector3.zero);
                Color attenuation = Color.black;

                // 说一下scatter 调用中的 ref 关键字   这个表示按照引用传递  因此后面运算中的attenuation 的值是材质本身设定的衰减值  并不是black
                // material 是 Metal  和 漫反射的毛绒球（兰伯特光照)  不同的材质对scatter 有不同的实现
                if (depth < MAX_SCATTER_TIME / 10 && record.material.scatter(ray, record, ref attenuation, ref r))
                {
                    Color c = GetColorForTestMetal(r, hitableList, depth + 1); //将镜面反射的光线再次进行碰撞计算
                    return new Color(c.r * attenuation.r, c.g * attenuation.g, c.b * attenuation.b);
                }
                else
                {
                    //假设已经反射了太多次，或者压根就没有发生反射，那么就认为黑了
                    return Color.black;
                }
            }
            float t = 0.5f * ray.normalDirection.y + 1f;
            return (1 - t) * new Color(1, 1, 1) + t * new Color(0.5f, 0.7f, 1);
        }

        Color[] CreateColorForTestMetal(int width, int height)
        {
            //视锥体的左下角、长宽和起始扫射点设定
            Vector3 lowLeftCorner = new Vector3(-4, -1, -1);
            Vector3 horizontal = new Vector3(8, 0, 0);
            Vector3 vertical = new Vector3(0, 2, 0);
            Vector3 original = new Vector3(0, 0, 0);
            int l = width * height;
            HitableList hitableList = new HitableList();
            hitableList.list.Add(new Sphere(new Vector3(0, 0, -1), 0.5f, new Lambertian(new Color(0.3f, 0.3f, 0.3f))));
            hitableList.list.Add(new Sphere(new Vector3(0, -100.5f, -1), 100f, new Lambertian(new Color(0.8f, 0.8f, 0.0f))));
            hitableList.list.Add(new Sphere(new Vector3(-1, 0, -1), 0.5f, new Metal(new Color(0.8f, 0.8f, 0.8f), 0.3f))); //Metal 的带个参数是albedo 反照率
            hitableList.list.Add(new Sphere(new Vector3(1, 0, -1), 0.5f, new Metal(new Color(0.8f, 0.8f, 0.8f), 0.0f))); // 设置fuzz为0  表示一个干净的镜面
            Color[] colors = new Color[l];

            //之前的视觉锥体变形比较厉害  现在把摄像机离远一点
            //右手坐标系  屏幕的z轴是 -1 
            SimpleCamera camera = new SimpleCamera(original + new Vector3(0, 0, 3), lowLeftCorner, horizontal, vertical);
            float recip_width = 1f / width;
            float recip_height = 1f / height;
            for (int j = height - 1; j >= 0; j--)
                for (int i = 0; i < width; i++)
                {
                    Color color = new Color(0, 0, 0);
                    for (int s = 0; s < SAMPLE / 10; s++)
                    {
                        // 光线的出发点就是摄像机
                        Ray r = camera.CreateRay((i + _M.R()) * recip_width, (j + _M.R()) * recip_height);
                        color += GetColorForTestMetal(r, hitableList, 0);
                    }
                    color *= SAMPLE_WEIGHT * 10f;
                    //为了使球体看起来更亮，改变gamma值
                    color = new Color(Mathf.Sqrt(color.r), Mathf.Sqrt(color.g), Mathf.Sqrt(color.b), 1f);
                    color.a = 1f;
                    colors[i + j * width] = color;
                }
            return colors;
        }
        #endregion
        #region 第九版（测试透明）      
        Color GetColorForTestDielectric(Ray ray, HitableList hitableList, int depth)
        {
            HitRecord record = new HitRecord();
            if (hitableList.Hit(ray, 0.0001f, float.MaxValue, ref record))
            {
                Ray r = new Ray(Vector3.zero, Vector3.zero);
                Color attenuation = Color.black;
                if (depth < MAX_SCATTER_TIME && record.material.scatter(ray, record, ref attenuation, ref r))
                {
                    Color c = GetColorForTestDielectric(r, hitableList, depth + 1);
                    return new Color(c.r * attenuation.r, c.g * attenuation.g, c.b * attenuation.b);
                }
                else
                {
                    //假设已经反射了太多次，或者压根就没有发生反射，那么就认为黑了
                    return Color.black;
                }
            }
            float t = 0.5f * ray.normalDirection.y + 1f;
            return (1 - t) * new Color(1, 1, 1) + t * new Color(0.5f, 0.7f, 1);
        }

        // 测试透明模型
        // 传统右手坐标系 Z值越小 离人眼越近
        // 之前调整参数 光源在 0 0 3 的位置
        // 屏幕Z轴在 -3 的位置          
        Color[] CreateColorForTestDielectric(int width, int height)
        {
            // 透明材料（例如水，玻璃和钻石）是电介质。 当光线射到它们上时，它分裂为反射射线和折射（透射）射线。 

            //视锥体的左下角、长宽和起始扫射点设定
            Vector3 lowLeftCorner = new Vector3(-2, -1, -3);
            Vector3 horizontal = new Vector3(4, 0, 0);
            Vector3 vertical = new Vector3(0, 2, 0);
            Vector3 original = new Vector3(0, 0, 0);
            int l = width * height;
            HitableList hitableList = new HitableList();

            // 漫反射 中间的红色金属球
            hitableList.list.Add(new Sphere(new Vector3(0, 0, -1), 0.5f, new Lambertian(new Color(0.8f, 0.3f, 0.3f))));

            // 漫反射 //底部的绿色球
            hitableList.list.Add(new Sphere(new Vector3(0, -100.5f, -1), 100f, new Lambertian(new Color(0.8f, 0.8f, 0.0f))));

            // 右侧的金属球
            hitableList.list.Add(new Sphere(new Vector3(1, 0, -1), 0.5f, new Metal(new Color(0.8f, 0.6f, 0.2f), 0f)));

            // 左侧透明球
            hitableList.list.Add(new Sphere(new Vector3(-1, 0, -1), 0.5f, new Dielectirc(1.5f)));
            // 1.3 是水的折射率  玻璃折射率是1.5

            Color[] colors = new Color[l];
            // 为了减小形变  让摄像头离屏幕远一点
            SimpleCamera camera = new SimpleCamera(original + new Vector3(0, 0, 3), lowLeftCorner, horizontal, vertical);
            float recip_width = 1f / width;
            float recip_height = 1f / height;
            for (int j = height - 1; j >= 0; j--)
                for (int i = 0; i < width; i++)
                {
                    Color color = new Color(0, 0, 0);
                    for (int s = 0; s < SAMPLE / 10; s++)
                    {
                        Ray r = camera.CreateRay((i + _M.R()) * recip_width, (j + _M.R()) * recip_height);
                        color += GetColorForTestDielectric(r, hitableList, 0);
                    }
                    color *= SAMPLE_WEIGHT * 10f;
                    //为了使球体看起来更亮，改变gamma值
                    color = new Color(Mathf.Sqrt(color.r), Mathf.Sqrt(color.g), Mathf.Sqrt(color.b), 1f);
                    color.a = 1f;
                    colors[i + j * width] = color;
                }
            return colors;
        }
        #endregion
        #region 第十版（测试FOV和相机角度）
        Color GetColorForTestCamera(Ray ray, HitableList hitableList, int depth)
        {
            HitRecord record = new HitRecord();
            if (hitableList.Hit(ray, 0.0001f, float.MaxValue, ref record))
            {
                Ray r = new Ray(Vector3.zero, Vector3.zero);
                Color attenuation = Color.black;
                if (depth < MAX_SCATTER_TIME && record.material.scatter(ray, record, ref attenuation, ref r))
                {
                    Color c = GetColorForTestCamera(r, hitableList, depth + 1);
                    return new Color(c.r * attenuation.r, c.g * attenuation.g, c.b * attenuation.b);
                }
                else
                {
                    //假设已经反射了太多次，或者压根就没有发生反射，那么就认为黑了
                    return Color.black;
                }
            }
            float t = 0.5f * ray.normalDirection.y + 1f;
            return (1 - t) * new Color(1, 1, 1) + t * new Color(0.5f, 0.7f, 1);
        }

        Color[] CreateColorForTestCamera(int width, int height)
        {
            //视锥体的左下角、长宽和起始扫射点设定
            Vector3 lowLeftCorner = new Vector3(-2, -1, -1);
            Vector3 horizontal = new Vector3(4, 0, 0);
            Vector3 vertical = new Vector3(0, 2, 0);
            Vector3 original = new Vector3(0, 0, 0);
            int l = width * height;
            HitableList hitableList = new HitableList();
            //紫色球
            hitableList.list.Add(new Sphere(new Vector3(0, 0, -1), 0.5f, new Lambertian(new Color(0.8f, 0.3f, 0.3f))));

            //下方的绿色球
            hitableList.list.Add(new Sphere(new Vector3(0, -100.5f, -1), 100f, new Lambertian(new Color(0.8f, 0.8f, 0.0f))));

            // 右侧的球 调整FOV后在最上方
            hitableList.list.Add(new Sphere(new Vector3(1, 0, -1), 0.5f, new Metal(new Color(0.2f, 0.6f, 0.9f), 0f)));

            // 左侧的球 调整FOV
            hitableList.list.Add(new Sphere(new Vector3(-1, 0, -1), 0.5f, new Dielectirc(1.5f)));

            Color[] colors = new Color[l];

            // 从左上角向下俯视
            // Vector3.up 就是 Vector3(0, 1, 0).

            //public Camera(Vector3 lookFrom, Vector3 lookat, Vector3 vup, float vfov, float aspect, float r = 0, float focus_dist = 1)
            //
            Camera camera = new Camera(new Vector3(-2, 2f, 1), new Vector3(0, 0, -1), Vector3.up, 75, width / height);
            float recip_width = 1f / width;
            float recip_height = 1f / height;
            for (int j = height - 1; j >= 0; j--)
                for (int i = 0; i < width; i++)
                {
                    Color color = new Color(0, 0, 0);
                    for (int s = 0; s < SAMPLE / 10; s++)
                    {
                        Ray r = camera.CreateRay((i + _M.R()) * recip_width, (j + _M.R()) * recip_height);
                        color += GetColorForTestCamera(r, hitableList, 0);
                    }
                    color *= SAMPLE_WEIGHT * 10;
                    //为了使球体看起来更亮，改变gamma值
                    color = new Color(Mathf.Sqrt(color.r), Mathf.Sqrt(color.g), Mathf.Sqrt(color.b), 1f);
                    color.a = 1f;
                    colors[i + j * width] = color;
                }
            return colors;
        }
        #endregion
        #region 第十一版（测试景深）
        Color GetColorForTestDefocus(Ray ray, HitableList hitableList, int depth)
        {
            HitRecord record = new HitRecord();
            if (hitableList.Hit(ray, 0.0001f, float.MaxValue, ref record))
            {
                Ray r = new Ray(Vector3.zero, Vector3.zero);
                Color attenuation = Color.black;
                if (depth < MAX_SCATTER_TIME && record.material.scatter(ray, record, ref attenuation, ref r))
                {
                    Color c = GetColorForTestDefocus(r, hitableList, depth + 1);
                    return new Color(c.r * attenuation.r, c.g * attenuation.g, c.b * attenuation.b);
                }
                else
                {
                    //假设已经反射了太多次，或者压根就没有发生反射，那么就认为黑了
                    return Color.black;
                }
            }
            float t = 0.5f * ray.normalDirection.y + 1f;
            return (1 - t) * new Color(1, 1, 1) + t * new Color(0.5f, 0.7f, 1);
        }

        Color[] CreateColorForTestDefocus(int width, int height)
        {
            //视锥体的左下角、长宽和起始扫射点设定
            Vector3 lowLeftCorner = new Vector3(-2, -1, -1);
            Vector3 horizontal = new Vector3(4, 0, 0);
            Vector3 vertical = new Vector3(0, 2, 0);
            Vector3 original = new Vector3(0, 0, 0);
            int l = width * height;
            HitableList hitableList = new HitableList();
            //这里注释的两句话是随机场景渲染用的……
            //HitableList hitableList = _M.CreateRandomScene();
            hitableList.list.Add(new Sphere(new Vector3(0, 0, -1), 0.5f, new Lambertian(new Color(0.2f, 0.2f, 0.8f))));
            hitableList.list.Add(new Sphere(new Vector3(0, -100.5f, -1), 100f, new Lambertian(new Color(0.8f, 0.8f, 0.0f))));
            hitableList.list.Add(new Sphere(new Vector3(1, 0, -1), 0.5f, new Metal(new Color(0.8f, 0.6f, 0.2f), 0f)));
            hitableList.list.Add(new Sphere(new Vector3(-1, 0, -1), 0.5f, new Dielectirc(1.5f))); //折射率都是大于1的
            Color[] colors = new Color[l];
            Vector3 from = new Vector3(10, 2f, -2);
            Vector3 to = new Vector3(0, 1, 0);
            Camera camera = new Camera(from, to, Vector3.up, 20, width / height, 2, (from - to).magnitude);
            //Camera camera = new Camera(from, to, Vector3.up, 35, width / height);
            float recip_width = 1f / width;
            float recip_height = 1f / height;
            for (int j = height - 1; j >= 0; j--)
            {
                for (int i = 0; i < width; i++)
                {
                    Color color = new Color(0, 0, 0);
                    for (int s = 0; s < SAMPLE; s++)
                    {
                        Ray r = camera.CreateRay((i + _M.R()) * recip_width, (j + _M.R()) * recip_height);
                        color += GetColorForTestDefocus(r, hitableList, 0);
                    }
                    color *= SAMPLE_WEIGHT;
                    //为了使球体看起来更亮，改变gamma值
                    color = new Color(Mathf.Sqrt(color.r), Mathf.Sqrt(color.g), Mathf.Sqrt(color.b), 1f);
                    color.a = 1f;
                    colors[i + j * width] = color;
                }
                EditorUtility.DisplayProgressBar("", "", j / (float)height);
            }
            EditorUtility.ClearProgressBar();
            return colors;
        }
        #endregion
        #region 第12 随机球大场景
        Color[] CreateColorForTestRandomBalls(int width, int height)
        {   
            // 在simpleCamer中视锥体的左下角、长宽和起始扫射点设定
            // Vector3 lowLeftCorner = new Vector3(-2, -1, -3);
            // 对于camera 视锥体的大小由lookAt 和 lookFrom 决定

            Vector3 horizontal = new Vector3(4, 0, 0);
            Vector3 vertical = new Vector3(0, 2, 0);
            Vector3 original = new Vector3(0, 0, 0);
            //在simpleCamera 中的摄像机的光线出发点一般就是origin
            //但是Camera中的ray出发点是 lookFrom

            int l = width * height;
            //这里注释的两句话是随机场景渲染用的……
            //三个大球的位置 金属球(0, 1, 0)  漫反射(-4, 1, 0)  玻璃球(4, 1, 0) 且半径都是1
            HitableList hitableList = _M.CreateRandomScene();
            Color[] colors = new Color[l];

            // from(0 1 0) to(10 1 0) 和 from(0 1 0) to (1 1 0) 理论上效果一样？Yes            
            // from(0 1 0) to (1 1 0) 和 from(2 1 0) to (3 1 0) 理论上效果一样？Yes
            // from(2.5f, 1, -6) to(2.5f, 1, 6)应该可以看到3个球？ Yes
            // from(2.5f, 3, -3) to(-5, 1, 3) //视野范围3的话(3+3)其实还不够  有交大形变
            // 所以后面采用了6
            Vector3 from = new Vector3(5f, 3, -6);
            Vector3 to = new Vector3(-5, 1, 6);


            // vup 这里并不是摄像头的位置 // vup 是用来控制摄像头围绕lookup的方向旋转的
            Camera camera = new Camera(from, to, new Vector3(0, 30, 0), 50, width / height);
            //Camera camera = new Camera(from, to, Vector3.up, 35, width / height);
            float recip_width = 1f / width;
            float recip_height = 1f / height;
            for (int j = 0; j <= height - 1; j++)
            {
                for (int i = 0; i < width; i++)
                {
                    Color color = new Color(0, 0, 0);
                    for (int s = 0; s < SAMPLE; s++)
                    {
                        Ray r = camera.CreateRay((i + _M.R()) * recip_width, (j + _M.R()) * recip_height);
                        color += GetColorForTestCamera(r, hitableList, 0);
                    }
                    color *= SAMPLE_WEIGHT;
                    //为了使球体看起来更亮，改变gamma值
                    color = new Color(Mathf.Sqrt(color.r), Mathf.Sqrt(color.g), Mathf.Sqrt(color.b), 1f);
                    color.a = 1f;
                    colors[i + j * width] = color;
                }
                EditorUtility.DisplayProgressBar("", "", j / (float)height);
            }
            EditorUtility.ClearProgressBar();
            return colors;
        }
        #endregion


        #region 图像生成
        void CreatePng(int width, int height, Color[] colors)
        {
            if (width * height != colors.Length)
            {
                EditorUtility.DisplayDialog("ERROR", "长宽与数组长度无法对应！", "ok");
                return;
            }
            Texture2D tex = new Texture2D(width, height, TextureFormat.ARGB32, false);
            tex.SetPixels(colors);
            tex.Apply();
            byte[] bytes = tex.EncodeToPNG();
            FileStream fs = new FileStream(IMG_PATH, FileMode.Create);
            BinaryWriter bw = new BinaryWriter(fs);
            bw.Write(bytes);
            fs.Close();
            bw.Close();
        }
        #endregion

    }
    #region 辅助类
    public class Ray
    {
        public Vector3 original;
        public Vector3 direction;
        public Vector3 normalDirection;
        public Ray(Vector3 o, Vector3 d)
        {
            original = o;
            direction = d;
            normalDirection = d.normalized;
        }

        public Vector3 GetPoint(float t)
        {
            return original + t * direction;
        }
    }


    // 延长长度t、交点p、交点处的法线方向
    public class HitRecord
    {
        public float t;
        public Vector3 p;
        public Vector3 normal;
        public Material material;
    }

    public abstract class Hitable
    {
        public abstract bool Hit(Ray ray, float t_min, float t_max, ref HitRecord rec);
    }

    public class HitableList : Hitable
    {
        public List<Hitable> list;
        public HitableList() { list = new List<Hitable>(); }
        /// <summary>
        /// 返回所有Hitable中最靠近射线源的命中信息
        /// </summary>
        /// <param name="ray"></param>
        /// <param name="t_min"></param>
        /// <param name="t_max"></param>
        /// <param name="rec"></param>
        /// <returns></returns>
        public override bool Hit(Ray ray, float t_min, float t_max, ref HitRecord rec)
        {
            HitRecord tempRecord = new HitRecord();
            bool hitAnything = false;
            float closest = t_max;
            foreach (var h in list)
            {
                if (h.Hit(ray, t_min, closest, ref tempRecord))
                {
                    hitAnything = true;
                    closest = tempRecord.t;
                    rec = tempRecord;
                }
            }
            return hitAnything;
        }
    }



    /// <summary>
    /// 前几个部分使用的简化版摄像机
    /// </summary>

    // 需要注明的是 所看到的图片 实际上是由camera的第一个参数 origin 向幕布发出光线形成的图
    public class SimpleCamera
    {
        public Vector3 position;
        public Vector3 lowLeftCorner;
        public Vector3 horizontal;
        public Vector3 vertical;
        public SimpleCamera(Vector3 pos, Vector3 llc, Vector3 hor, Vector3 ver)
        {
            position = pos;
            lowLeftCorner = llc;
            horizontal = hor; //逻辑上的长和宽
            vertical = ver; //逻辑上的长和宽
        }

        // u v 表示相机位置的百分比
        public Ray CreateRay(float u, float v)
        {
            return new Ray(position, lowLeftCorner + u * horizontal + v * vertical - position);
        }
    }

    public class Camera
    {
        public Vector3 position;
        public Vector3 lowLeftCorner;
        public Vector3 horizontal;
        public Vector3 vertical;
        public Vector3 u, v, w;
        public float radius;

        ///此处FOV是欧拉角
        // vfov表示视野范围  数值越大
        // vup 是摄像头所在的位置
        // lookFrom 和 lookat 共同决定摄像头的的朝向  也就是相机往哪边看的问题
        public Camera(Vector3 lookFrom, Vector3 lookat, Vector3 vup, float vfov, float aspect, float r = 0, float focus_dist = 1)
        {
            radius = r * 0.5f;
            float unitAngle = Mathf.PI / 180f * vfov;
            float halfHeight = Mathf.Tan(unitAngle * 0.5f);
            float halfWidth = aspect * halfHeight;
            position = lookFrom;

            // w u v 的含义在书中有图片说明
            w = (lookat - lookFrom).normalized;// 在书中用的 lookFrom - lookat
            // w = (lookFrom - lookat).normalized;
            u = Vector3.Cross(vup, w).normalized;
            v = Vector3.Cross(w, u).normalized;
            lowLeftCorner = lookFrom + w * focus_dist - halfWidth * u * focus_dist - halfHeight * v * focus_dist;
            horizontal = 2 * halfWidth * focus_dist * u;
            vertical = 2 * halfHeight * focus_dist * v;
        }
        public Ray CreateRay(float x, float y)
        {
            ///假如光圈为0就不随机了，节省资源
            if (radius == 0f)
                return new Ray(position, lowLeftCorner + x * horizontal + y * vertical - position);
            else
            {
                Vector3 rd = radius * _M.GetRandomPointInUnitDisk();
                Vector3 offset = rd.x * u + rd.y * v;
                return new Ray(position + offset, lowLeftCorner + x * horizontal + y * vertical - position - offset);
            }
        }
    }
    public abstract class Material
    {
        /// <summary>
        /// 材质表面发生的光线变化过程
        /// </summary>
        /// <param name="rayIn"></param>
        /// <param name="record"></param>
        /// <param name="attenuation">衰减</param>
        /// <param name="scattered"></param>
        /// <returns>是否发生了光线变化</returns>
        public abstract bool scatter(Ray rayIn, HitRecord record, ref Color attenuation, ref Ray scattered);
    }

    public static class _M
    {
        public static Vector3 GetRandomPointInUnitSphere()
        {
            Vector3 p = 2f * new Vector3(_M.R(), _M.R(), _M.R()) - Vector3.one;
            p = p.normalized * _M.R();
            return p;
        }
        /// <summary>
        /// 这个取的是圆面而不是球
        /// </summary>
        /// <returns></returns>
        public static Vector3 GetRandomPointInUnitDisk()
        {
            Vector3 p = 2f * new Vector3(_M.R(), _M.R(), 0) - new Vector3(1, 1, 0);
            p = p.normalized * _M.R();
            return p;
        }

        public static Vector3 reflect(Vector3 vin, Vector3 normal)
        {
            return vin - 2 * Vector3.Dot(vin, normal) * normal;
        }

        public static bool refract(Vector3 vin, Vector3 normal, float ni_no, ref Vector3 refracted)
        {
            Vector3 uvin = vin.normalized;
            // https://zhuanlan.zhihu.com/p/31127076
            // https://blog.csdn.net/yinhun2012/article/details/79472364
            float dt = Vector3.Dot(uvin, normal);
            float discrimination = 1 - ni_no * ni_no * (1 - dt * dt);
            if (discrimination > 0)
            {
                refracted = ni_no * (uvin - normal * dt) - normal * Mathf.Sqrt(discrimination);
                return true;
            }
            return false;
        }


        // 菲涅尔（发音为Fresnel）方程描述的是被反射的光线对比光线被折射的部分所占的比率，这个比率会随着我们观察的角度不同而不同。        
        // 反射系数的求解是是一个非常复杂的过程，Christophe Schlick这个人提供一个逼近公式，这个公式被称为“ChristopheSchlick’s Approximation”。Wiki链接：
        // https://en.wikipedia.org/wiki/Schlick%27s_approximation

        // 也可以参考 https://learnopengl-cn.github.io/07%20PBR/01%20Theory/

        /// <summary>
        /// Schilick近似菲涅尔反射
        /// </summary>
        /// <returns>返回的是菲涅尔反射比</returns>
        public static float schlick(float cos, float ref_idx)
        {
            float r0 = (1 - ref_idx) / (1 + ref_idx);
            r0 *= r0;
            return r0 + (1 - r0) * Mathf.Pow((1 - cos), 5);
        }

        public static float R()
        {
            return Random.Range(0f, 1f);
        }

        public static HitableList CreateRandomScene()
        {
            HitableList list = new HitableList();
            list.list.Add(new Sphere(new Vector3(0f, -1000f, 0f), 1000f, new Lambertian(Color.gray)));
            for (int a = -4; a < 4; a++)
                for (int b = -4; b < 4; b++)
                {
                    float choose_mat = R();
                    Vector3 center = new Vector3(a + 0.9f * R(), 0.2f, b + 0.9f * R());
                    if (Vector3.Distance(center, new Vector3(4, 0.2f, 4)) > 0.9f)
                    {
                        if (choose_mat < 0.8f)
                            list.list.Add(new Sphere(center, 0.2f, new Lambertian(new Color(R() * R(), R() * R(), R() * R()))));
                        else if (choose_mat < 0.95f)
                            list.list.Add(new Sphere(center, 0.2f, new Metal(new Color(0.5f + 0.5f * R(), 0.5f + 0.5f * R(), 0.5f + 0.5f * R()), 0.5f * R())));
                        else
                            list.list.Add(new Sphere(center, 0.2f, new Dielectirc(1.5f)));
                    }
                }
            list.list.Add(new Sphere(new Vector3(0, 1, 0), 1, new Metal(new Color(0.1f, 0.6f, 0.5f), 0.01f)));
            list.list.Add(new Sphere(new Vector3(-4, 1, 0), 1, new Lambertian(new Color(0.9f, 0.2f, 0.1f))));
            list.list.Add(new Sphere(new Vector3(4, 1, 0), 1, new Dielectirc(1.5f)));
            return list;
        }

    }
    #endregion
    #region 各式各样的SDF类
    /// <summary>
    /// 前几个测试中用到的简化版SDF球
    /// </summary>
    public class SimpleSphere : Hitable
    {
        public Vector3 center;
        public float radius;
        public SimpleSphere(Vector3 cen, float rad)
        {
            center = cen; radius = rad;
        }
        public override bool Hit(Ray ray, float t_min, float t_max, ref HitRecord rec)
        {
            var oc = ray.original - center;
            float a = Vector3.Dot(ray.direction, ray.direction);
            float b = 2f * Vector3.Dot(oc, ray.direction);
            float c = Vector3.Dot(oc, oc) - radius * radius;
            //实际上是判断这个方程有没有根，如果有2个根就是击中
            float discriminant = b * b - 4 * a * c;
            if (discriminant > 0)
            {
                //带入并计算出最靠近射线源的点
                //这里就是一元二次方程球根公式的解
                // Debug.LogFormat("Hitable discriminant {0}", discriminant);
                // Log File locates in ~/Library/Logs/Unity

                float temp = (-b - Mathf.Sqrt(discriminant)) / a * 0.5f;
                if (temp < t_max && temp > t_min)
                {
                    rec.t = temp;
                    rec.p = ray.GetPoint(rec.t);
                    rec.normal = (rec.p - center).normalized;
                    return true;
                }
                //否则就计算远离射线源的点
                temp = (-b + Mathf.Sqrt(discriminant)) / a * 0.5f;
                if (temp < t_max && temp > t_min)
                {
                    rec.t = temp;
                    rec.p = ray.GetPoint(rec.t);
                    rec.normal = (rec.p - center).normalized;
                    return true;
                }
            }
            return false;
        }
    }

    public class Sphere : Hitable
    {
        public Vector3 center;
        public float radius;
        public Material material;

        public Sphere(Vector3 cen, float rad, Material mat)
        {
            center = cen; radius = rad; material = mat;
        }

        public override bool Hit(Ray ray, float t_min, float t_max, ref HitRecord rec)
        {
            var oc = ray.original - center;
            float a = Vector3.Dot(ray.direction, ray.direction);
            float b = 2f * Vector3.Dot(oc, ray.direction);
            float c = Vector3.Dot(oc, oc) - radius * radius;
            //实际上是判断这个方程有没有根，如果有2个根就是击中
            float discriminant = b * b - 4 * a * c;
            if (discriminant > 0)
            {
                //带入并计算出最靠近射线源的点
                float temp = (-b - Mathf.Sqrt(discriminant)) / a * 0.5f;
                if (temp < t_max && temp > t_min)
                {
                    rec.t = temp;
                    rec.p = ray.GetPoint(rec.t);
                    rec.normal = (rec.p - center).normalized;
                    rec.material = material;
                    return true;
                }
                //否则就计算远离射线源的点
                temp = (-b + Mathf.Sqrt(discriminant)) / a * 0.5f;
                if (temp < t_max && temp > t_min)
                {
                    rec.t = temp;
                    rec.p = ray.GetPoint(rec.t);
                    rec.normal = (rec.p - center).normalized;
                    rec.material = material;
                    return true;
                }
            }
            return false;
        }
    }
    #endregion
    #region 各式各样的散射模型
    /// <summary>
    /// 理想的漫反射模型
    /// </summary>
    public class Lambertian : Material
    {
        Color albedo;
        public override bool scatter(Ray rayIn, HitRecord record, ref Color attenuation, ref Ray scattered)
        {
            Vector3 target = record.p + record.normal + _M.GetRandomPointInUnitSphere();
            scattered = new Ray(record.p, target - record.p);
            attenuation = albedo;
            return true;
        }
        public Lambertian(Color a) { albedo = a; }
    }
    /// <summary>
    /// 理想的镜面反射模型
    /// </summary>
    public class Metal : Material
    {
        Color albedo;
        float fuzz;
        public Metal(Color a, float f = 0f) { albedo = a; fuzz = f < 1 ? f : 1; }
        public override bool scatter(Ray rayIn, HitRecord record, ref Color attenuation, ref Ray scattered)
        {
            Vector3 reflected = _M.reflect(rayIn.normalDirection, record.normal);
            scattered = new Ray(record.p, reflected + fuzz * _M.GetRandomPointInUnitSphere());
            attenuation = albedo;
            return Vector3.Dot(scattered.direction, record.normal) > 0; //点积>0 表示方向相近
        }
    }
    /// <summary>
    /// 透明折射模型
    /// </summary>
    public class Dielectirc : Material
    {
        //相对空气的折射率 //一般认为空气折射率是1
        float ref_idx;
        public Dielectirc(float ri) { ref_idx = ri; }
        public override bool scatter(Ray rayIn, HitRecord record, ref Color attenuation, ref Ray scattered)
        {
            Vector3 outNormal;

            // reflect 中实际上是点乘  因此第一个参数是否归一化理论不影响
            Vector3 reflected = _M.reflect(rayIn.normalDirection, record.normal);


            //透明的物体当然不会吸收任何光线
            attenuation = Color.white;
            float ni_no = 1f;
            Vector3 refracted = Vector3.zero;
            float cos = 0;
            //反射比
            float reflect_prob = 0;

            //回忆一下二次公式 最常见的情况 一个光束 和 球 有两个交点
            //假如光线是从介质内向介质外传播，那么法线就要反转一下

            // 点积 a.b < 0 则 ab 是钝角(基本不同方向)
            // 注意一下 record的法线都是外表面的法线
            // 当光线和外表面法线几乎同方向的时候
            // 表示从光线正从球内射出
            if (Vector3.Dot(rayIn.direction, record.normal) > 0)
            {
                // 那么法线就要反转一下
                outNormal = -record.normal;
                ni_no = ref_idx;//相对空气折射率

                // 斯涅尔定律
                // n1*sinθ1 = n2*sinθ2
                // θ1是入射角  θ2是折射角
                // n1  n2 分别是折射率
                cos = ni_no * Vector3.Dot(rayIn.normalDirection, record.normal);
            }
            else
            {
                outNormal = record.normal;
                ni_no = 1f / ref_idx;
                cos = -Vector3.Dot(rayIn.normalDirection, record.normal);
            }
            //如果没发生折射，就用反射
            if (_M.refract(rayIn.direction, outNormal, ni_no, ref refracted))
            {
                reflect_prob = _M.schlick(cos, ref_idx);
            }
            else
            {
                //此时反射比为100%
                // reflect_prob = 1;
                reflect_prob = _M.schlick(cos, ref_idx);
            }


            //因为一条光线只会采样一个点，所以这里就用蒙特卡洛模拟的方法，用概率去决定数值
            if (_M.R() <= reflect_prob)
            {
                scattered = new Ray(record.p, reflected);
            }
            else
            {
                scattered = new Ray(record.p, refracted);
            }
            return true;
        }
    }
    #endregion
}

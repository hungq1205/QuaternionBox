using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace QuaternionTest
{
    internal class Program
    {
        public const int RESOLUTION_SCALAR = 10;
        public const float SQUARE_WIDTH = 30f;

        static void Main(string[] args)
        {
            Console.CursorVisible = false;

            bool useTargetedRotation = false;
            double angle = 5, targetedAngle = 90;

            Quaternion rotationUp = Quaternion.CreateOrthogonalRotation(Direction.Up, angle);
            Quaternion rotationDown = Quaternion.CreateOrthogonalRotation(Direction.Down, angle);
            Quaternion rotationLeft = Quaternion.CreateOrthogonalRotation(Direction.Left, angle);
            Quaternion rotationRight = Quaternion.CreateOrthogonalRotation(Direction.Right, angle);

            StringBuilder buffer = new StringBuilder();
            Space space = new Space(4, 4, RESOLUTION_SCALAR);
            Vector shapeForward = Vector.forward;
            Vector targetForward = new Vector(1, 0, 0);
            //List<Vector> inputVectors = new List<Vector>();

            Vector[] squareVertices =
            {
                new Vector(-1, 1, 1),
                new Vector(1, 1, 1),
                new Vector(1, -1, 1),
                new Vector(-1, -1, 1),
                new Vector(-1, -1, -1),
                new Vector(1, -1, -1),
                new Vector(1, 1, -1),
                new Vector(-1, 1, -1)
            };

            for(int i = 0; i < squareVertices.Length; i++)
                squareVertices[i] *= SQUARE_WIDTH / 2;

            int[] vertexIndices =
            {
                4, 1, 3,
                3, 1, 2,
                5, 4, 3,
                6, 5, 3,
                1, 8, 2,
                2, 8, 7,
                7, 8, 6,
                8, 5, 6,
                4, 8, 1,
                4, 5, 8,
                3, 2, 7,
                3, 7, 6
            };

            // miscalculated indices temporary fix
            for (int i = 0; i < vertexIndices.Length; i++)
                vertexIndices[i]--;

            for (int i = 0; i < vertexIndices.Length; i += 3)
            {
                char notion = '0';

                if (i < 6)
                    notion = '1';
                else if (i < 12)
                    notion = '2';
                else if (i < 18)
                    notion = '3';
                else if (i < 24)
                    notion = '4';
                else if (i < 30)
                    notion = '5';
                else if (i < 36)
                    notion = '6';

                space.AddTriangle(squareVertices[vertexIndices[i]], squareVertices[vertexIndices[i + 1]], squareVertices[vertexIndices[i + 2]], notion);
            }

            while (true)
            {
                #region Manual input
                //    do
                //    {
                //        double inputX = 0, inputY = 0, inputZ = 0;
                //        bool isInputValid = false;

                //        do
                //        {
                //            Console.WriteLine("Insert vector no {0}: ", inputVectors.Count + 1);

                //            try
                //            {
                //                WriteTab("x: ", 1);
                //                inputX = int.Parse(Console.ReadLine());
                //                WriteTab("y: ", 1);
                //                inputY = int.Parse(Console.ReadLine());
                //                WriteTab("z: ", 1);
                //                inputZ = int.Parse(Console.ReadLine());

                //                isInputValid = true;
                //            }
                //            catch
                //            {
                //                Console.Clear();
                //                WriteLinePad("INVALID INPUT");

                //                foreach (Vector vector in inputVectors)
                //                    Console.WriteLine(vector);
                //            }
                //        } while (!isInputValid);

                //        Vector inputVector = new Vector(inputX, inputY, inputZ);
                //        inputVectors.Add(inputVector);
                //    } while (inputVectors.Count < 3);
                #endregion

                while (true)
                {
                    Quaternion rotation = Quaternion.identity;
                    ConsoleKeyInfo inputDirChar;
                    bool rotatePressed = true;
                    do
                    {
                        PointNotion[,] flattenSpace = space.FlattenXY(true);

                        for (int y = 0; y < space.flattenHeight * 2; y++)
                        {
                            for (int x = 0; x < space.flattenWidth * 2; x++)
                            {
                                buffer.Append(flattenSpace[x, y].left);
                                buffer.Append(flattenSpace[x, y].right);
                            }

                            buffer.AppendLine();
                        }
                        WriteLinePad(String.Format("ROTATE BY {0}", angle), buffer);
                        Console.Write(buffer);
                        buffer.Clear();

                        inputDirChar = Console.ReadKey();
                        rotatePressed = true;

                        if (inputDirChar.Key == ConsoleKey.Spacebar)
                        {
                            Console.Write("Insert angle: ");

                            bool validInput = false;
                            do
                            {
                                try
                                {
                                    angle = int.Parse(Console.ReadLine());
                                    validInput = true;
                                }
                                catch
                                {
                                    Console.WriteLine("Invalid input");
                                    validInput = false;
                                }
                            } while (!validInput);

                            rotatePressed = false;
                            buffer.Clear();
                            Console.Clear();
                        }
                        else if (inputDirChar.Key == ConsoleKey.Tab)
                        {
                            useTargetedRotation = !useTargetedRotation;
                            Console.WriteLine("Changed to {0}:", useTargetedRotation ? "Targeted rotation" : "Continuous rotation");

                            if (useTargetedRotation)
                            {
                                WriteTab("Insert targeted directional angle: ", 2);

                                bool validInput = false;
                                do
                                {
                                    try
                                    {
                                        targetedAngle = int.Parse(Console.ReadLine());
                                        validInput = true;
                                    }
                                    catch
                                    {
                                        Console.WriteLine("Invalid input");
                                        validInput = false;
                                    }
                                } while (!validInput);

                                rotatePressed = false;
                                buffer.Clear();
                                Console.Clear();
                            }
                        }
                        else
                        {
                            if (useTargetedRotation)
                                rotation = Quaternion.RotationTowards(shapeForward, CheckRotationInput(inputDirChar, targetedAngle) * Vector.forward, angle);
                            else
                                rotation = CheckRotationInput(inputDirChar, angle);

                            if (rotation == Quaternion.identity)
                            {
                                rotatePressed = false;
                                buffer.Clear();
                                Console.Clear();
                            }
                        }
                        Console.SetCursorPosition(0, 0);
                    } while (!rotatePressed);

                    #region Auto rotate
                    do
                    {
                        Framer framer = new Framer(() =>
                        {
                            RotateAndPrint(space, rotation);
                            WriteLinePad("ANY KEY TO CANCEL");
                            shapeForward = rotation * shapeForward;
                            if (useTargetedRotation)
                                rotation = Quaternion.RotationTowards(shapeForward, CheckRotationInput(inputDirChar, targetedAngle) * Vector.forward, angle);
                        }, 30);
                        framer.Start();

                        inputDirChar = Console.ReadKey();
                        rotation = CheckRotationInput(inputDirChar, angle);

                        framer.Stop();
                    } while (rotation != Quaternion.identity);
                    Console.SetCursorPosition(0, 0);
                    #endregion

                    #region Rotate once
                    //RotateAndPrint(space, rotation);
                    //Console.SetCursorPosition(0, 0);
                    #endregion
                }

                //Console.WriteLine("Output vector: {0}", (rotation * vectorRotation * rotation.conjugated).vectorPart);
                WriteLinePad("ANY KEY");
                Console.ReadKey();

                Console.Clear();
            }
        }

        static Quaternion CheckRotationInput(ConsoleKeyInfo inputDirChar, double angle)
        {
            switch (inputDirChar.Key)
            {
                case ConsoleKey.W:
                case ConsoleKey.UpArrow:
                    return Quaternion.CreateOrthogonalRotation(Direction.Up, angle);
                case ConsoleKey.X:
                case ConsoleKey.DownArrow:
                    return Quaternion.CreateOrthogonalRotation(Direction.Down, angle);
                case ConsoleKey.A:
                case ConsoleKey.LeftArrow:
                    return Quaternion.CreateOrthogonalRotation(Direction.Left, angle);
                case ConsoleKey.D:
                case ConsoleKey.RightArrow:
                    return Quaternion.CreateOrthogonalRotation(Direction.Right, angle);
                case ConsoleKey.NumPad8:
                    return Quaternion.CreateOrthogonalRotation(Direction.Up, angle);
                case ConsoleKey.NumPad2:
                    return Quaternion.CreateOrthogonalRotation(Direction.Down, angle);
                case ConsoleKey.NumPad4:
                    return Quaternion.CreateOrthogonalRotation(Direction.Left, angle);
                case ConsoleKey.NumPad6:
                    return Quaternion.CreateOrthogonalRotation(Direction.Right, angle);
                case ConsoleKey.Z:
                case ConsoleKey.NumPad1:
                    return Quaternion.CreateOrthogonalRotation(Direction.Down, angle / 1.41) * Quaternion.CreateOrthogonalRotation(Direction.Left, angle / 1.41);
                case ConsoleKey.C:
                case ConsoleKey.NumPad3:
                    return Quaternion.CreateOrthogonalRotation(Direction.Down, angle / 1.41) * Quaternion.CreateOrthogonalRotation(Direction.Right, angle / 1.41);
                case ConsoleKey.E:
                case ConsoleKey.NumPad9:
                    return Quaternion.CreateOrthogonalRotation(Direction.Up, angle / 1.41) * Quaternion.CreateOrthogonalRotation(Direction.Right, angle / 1.41);
                case ConsoleKey.Q:
                case ConsoleKey.NumPad7:
                    return Quaternion.CreateOrthogonalRotation(Direction.Up, angle / 1.41) * Quaternion.CreateOrthogonalRotation(Direction.Left, angle / 1.41);
            }

            return Quaternion.identity;
        }

        static void RotateAndPrint(Space space, Quaternion rotation)
        {
            PointNotion[,] flattenSpace = space.FlattenXY(true);
            StringBuilder buffer = new StringBuilder();

            for (int i = 0; i < space.triangles.Count; i++)
                space.triangles[i] = Quaternion.RotateWith(space.triangles[i], rotation);

            buffer.Clear();
            Console.SetCursorPosition(0, 0);

            for (int y = 0; y < space.flattenHeight * 2; y++)
            {
                for (int x = 0; x < space.flattenWidth * 2; x++)
                {
                    buffer.Append(flattenSpace[x, y].left);
                    buffer.Append(flattenSpace[x, y].right);
                }

                buffer.AppendLine();
            }
            Console.Write(buffer);
        }

        #region PrintHelper

        public static void WritePad(string value, StringBuilder buffer, uint padNum = 16 * RESOLUTION_SCALAR, char padChar = '-')
        {
            char[] chars = value.ToCharArray();

            for(int i = 0; i < padNum; i++)
            {
                if (i >= padNum / 2 - chars.Length / 2 && i - padNum / 2 + chars.Length / 2 < chars.Length)
                    buffer.Append(chars[i - padNum / 2 + chars.Length / 2]);
                else
                    buffer.Append(padChar);
            }
        }

        public static void WriteLinePad(string value, StringBuilder buffer, uint padNum = 16 * RESOLUTION_SCALAR, char padChar = '-')
        {
            char[] chars = value.ToCharArray();

            for(int i = 0; i < padNum; i++)
            {
                if (i >= padNum / 2 - chars.Length / 2 && i - padNum / 2 + chars.Length / 2 < chars.Length)
                    buffer.Append(chars[i - padNum / 2 + chars.Length / 2]);
                else
                    buffer.Append(padChar);
            }

            buffer.AppendLine();
        }

        public static void WritePad(string value, uint padNum = 16 * RESOLUTION_SCALAR, char padChar = '-')
        {
            char[] chars = value.ToCharArray();

            for(int i = 0; i < padNum; i++)
            {
                if (i >= padNum / 2 - chars.Length / 2 && i - padNum / 2 + chars.Length / 2 < chars.Length)
                    Console.Write(chars[i - padNum / 2 + chars.Length / 2]);
                else
                    Console.Write(padChar);
            }
        }
        
        public static void WriteLinePad(string value, uint padNum = 16 * RESOLUTION_SCALAR, char padChar = '-')
        {
            char[] chars = value.ToCharArray();

            for(int i = 0; i < padNum; i++)
            {
                if (i >= padNum / 2 - chars.Length / 2 && i - padNum / 2 + chars.Length / 2 < chars.Length)
                    Console.Write(chars[i - padNum / 2 + chars.Length / 2]);
                else
                    Console.Write(padChar);
            }

            Console.WriteLine();
        }

        public static void WriteTab(string value, uint tab = 0)
        {
            for (int i = 0; i < tab; i++)
                Console.Write("\t");
            Console.Write(value);
        }

        public static void WriteLineTab(string value, uint tab = 0)
        {
            for (int i = 0; i < tab; i++)
                Console.Write("\t");
            Console.WriteLine(value);
        }

        #endregion
    }

    public class Framer
    {
        public int fps = 30;

        public Action action;

        CancellationTokenSource tokenSource;
        Task task;

        public float elapsedSec { get; private set; }

        public Framer(Action action, int fps = 30)
        {
            this.fps = fps;
            this.action = action;
        }

        public void Start()
        {
            tokenSource = new CancellationTokenSource();

            task = Task.Run(Timer, tokenSource.Token);
            elapsedSec = 0;
        }

        public void Stop()
        {
            tokenSource.Cancel();
            task.Wait();
        }

        void Timer()
        {
            Thread.Sleep(1000 / fps);
            while (!tokenSource.IsCancellationRequested)
            {
                elapsedSec += 1 / fps;
                action();
                Thread.Sleep(1000 / fps);
            }
            tokenSource.Dispose();
        }
    }

    public class Space
    {
        public List<OrderedTriangle> triangles;
        public int flattenWidth = 10, flattenHeight = 10;

        int _resolutionScalar = 1;
        public int resolutionScalar
        {
            get { return _resolutionScalar; }
            set
            {
                flattenWidth *= value;
                flattenHeight *= value;

                _resolutionScalar = value;
            }
        }

        public Space(int flattenWidth = 10, int flattenHeight = 10, int resolutionScalar = 1)
        {
            this.triangles = new List<OrderedTriangle>();
            this.flattenWidth = flattenWidth;
            this.flattenHeight = flattenHeight;
            this.resolutionScalar = resolutionScalar;
        }

        public PointNotion[,] FlattenXY(bool showAxis = false)
        {
            PointNotion[,] flattenSpace = new PointNotion[flattenWidth * 2, flattenHeight * 2];

            for (int i = 0; i < flattenSpace.GetLength(0); i++)
                for (int j = 0; j < flattenSpace.GetLength(1); j++)
                    flattenSpace[i, j] = new PointNotion();

            if (showAxis)
            {
                for(int x = 0; x < flattenWidth * 2; x++)
                    flattenSpace[x, flattenHeight] = '-';
                for(int y = 0; y < flattenHeight * 2; y++)
                    flattenSpace[flattenWidth, y] = '|';
                flattenSpace[flattenWidth, flattenHeight] = '+';
            }

            try
            {
                foreach (OrderedTriangle triangle in triangles)
                {
                    if (Vector.Angle(triangle.normal, Vector.forward) < 90 || triangle.normal == Vector.forward)
                        RenderTriangle(triangle, flattenSpace);
                }
            } catch { }

            return flattenSpace;
        }

        public void RenderTriangle(OrderedTriangle triangle, PointNotion[,] surface)
        {
            if (surface == null)
                return;

            OrderedTriangle flatten = triangle.flatten;

            Vector ab = flatten.ab;
            Vector ac = -flatten.ca;
            Vector abStep = (ab / ab.magnitude) * (2 / (float)resolutionScalar);
            Vector acStep = (ac / ac.magnitude) * (2 / (float)resolutionScalar);

            int baStepNum = (int)Math.Round(Math.Sqrt(ab.sqrMagnitude / abStep.sqrMagnitude));
            int caStepNum = (int)Math.Round(Math.Sqrt(ac.sqrMagnitude / acStep.sqrMagnitude));

            for (int i = 1; i <= baStepNum; i++)
            {
                for(int j = 1; j <= caStepNum; j++)
                {
                    Vector currentDraw = flatten.vertexA + abStep * i + acStep * j;
                    Vector roundedVector = new Vector((int)Math.Round(currentDraw.x), (int)Math.Round(currentDraw.y));

                    if (roundedVector.x < -flattenWidth || roundedVector.y < -flattenHeight || roundedVector.x > flattenWidth || roundedVector.y > flattenHeight)
                        break;
                    else if (OrderedTriangle.CheckInsidePoint(currentDraw, flatten)) 
                    {
                        PointNotion point = surface[(int)roundedVector.x + flattenWidth, flattenHeight - (int)roundedVector.y];
                        double difference = Math.Abs(currentDraw.x - Math.Floor(currentDraw.x));

                        if (point == null)
                            point = new PointNotion();

                        if (difference > 0.53d)
                            point.SetLeftNotion(triangle.notion);
                        else if (difference < 0.47d)
                            point.SetRightNotion(triangle.notion);
                        else
                            point = triangle.notion;
                    }
                }
            }
        }

        public void AddTriangle(OrderedTriangle triangle)
        {
            triangles.Add(triangle);
        }

        public void AddTriangle(Vector vertexA, Vector vertexB, Vector vertexC, char notion = '#')
        {
            triangles.Add(new OrderedTriangle(vertexA, vertexB, vertexC, notion));
        }
    }

    public struct OrderedTriangle 
    {
        public Vector vertexA, vertexB, vertexC;
        public char notion;

        public double area { get => (ab * bc).magnitude / 2; }

        public Vector normal { get => (-ab * bc).normalized; }

        public Vector ab { get => vertexB - vertexA; }
        public Vector bc { get => vertexC - vertexB; }
        public Vector ca { get => vertexA - vertexC; }

        public OrderedTriangle flatten { get => new OrderedTriangle(vertexA.flatten, vertexB.flatten, vertexC.flatten, notion); }

        public OrderedTriangle(Vector vertexA, Vector vertexB, Vector vertexC, char notion = '#')
        {
            this.vertexA = vertexA;
            this.vertexB = vertexB;
            this.vertexC = vertexC;
            this.notion = notion;
        }

        public void Foreach(Action<Vector> action)
        {
            action(vertexA);
            action(vertexB);
            action(vertexC);
        }

        public static bool CheckInsidePoint(Vector point, OrderedTriangle triangle)
        {
            Vector barycentric = AbsBarycentric(point, triangle);

            if (barycentric.x + barycentric.y + barycentric.z < 1 + 0.000001 && barycentric.x + barycentric.y + barycentric.z > 1 - 0.000001)
                return true;

            return false;
        }

        public static Vector AbsBarycentric(Vector point, OrderedTriangle triangle)
        {
            Vector ap = point - triangle.vertexA;
            Vector bp = point - triangle.vertexB;
            Vector cp = point - triangle.vertexC;

            double sqrArea = (triangle.ab * triangle.ca).sqrMagnitude;

            double alpha = Math.Sqrt((bp * cp).sqrMagnitude / sqrArea);
            double beta = Math.Sqrt((ap * cp).sqrMagnitude / sqrArea);
            double gamma = Math.Sqrt((ap * bp).sqrMagnitude / sqrArea);

            return new Vector(alpha, beta, gamma);
        }

        public override string ToString()
        {
            return String.Format("Triangle:\n\t{0},\n\t{1},\n\t{2}", this.vertexA, this.vertexB, this.vertexC);
        }
    }

    public class PointNotion
    {
        public char left, right;

        public PointNotion()
        {
            left = right = ' ';
        }

        public PointNotion(char left, char right)
        {
            this.left = left;
            this.right = right;
        }

        public void SetLeftNotion(char notion)
        {
            left = notion;
        }

        public void SetRightNotion(char notion)
        {
            right = notion;
        }

        public static implicit operator PointNotion(char notion) => new PointNotion(notion, notion);
    }

    public enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }

    public struct Quaternion
    {
        public double w, x, y, z;

        public static readonly Quaternion identity = new Quaternion(1, Vector.zero);

        public double sqrMagnitude
        {
            get { return x * x + y * y + z * z + w * w; }
        }

        public double magnitude
        {
            get { return Math.Sqrt(sqrMagnitude); }
        }

        public Quaternion conjugated
        {
            get { return new Quaternion(w, -x, -y, -z); }
        }

        public Quaternion normalized
        {
            get { return this / magnitude; }
        }

        public Vector vectorPart
        {
            get { return new Vector(x, y, z); }
        }

        public Quaternion(double w, double x, double y, double z)
        {
            this.w = w;
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public Quaternion(double real, Vector vectorPart)
        {
            this.w = real;
            this.x = vectorPart.x;
            this.y = vectorPart.y;
            this.z = vectorPart.z;
        }

        public static Quaternion Lerp(Quaternion a, Quaternion b, float t)
        {
            float t_ = 1 - t;

            return (new Quaternion(
                t_ * a.w + t * b.w,
                t_ * a.x + t * b.x,
                t_ * a.y + t * b.y,
                t_ * a.z + t * b.z
            )).normalized;
        }

        public static Quaternion CreateOrthogonalRotation(Direction direction, double angle)
        {
            double sin = Math.Sin(angle * (Math.PI / 180)), cos = Math.Cos(angle * (Math.PI / 180));

            switch (direction)
            {
                case Direction.Up:
                    return Quaternion.RotationOf(Vector.forward, new Vector(0, sin, cos));
                case Direction.Down:
                    return Quaternion.RotationOf(Vector.forward, new Vector(0, -sin, cos));
                case Direction.Left:
                    return Quaternion.RotationOf(Vector.forward, new Vector(-sin, 0, cos));
                case Direction.Right:
                    return Quaternion.RotationOf(Vector.forward, new Vector(sin, 0, cos));
            }

            throw new Exception("Invalid direction or angle");
        }

        public static OrderedTriangle RotateWith(OrderedTriangle triangle, Quaternion quaternion)
        {
            return new OrderedTriangle(
                quaternion * triangle.vertexA,
                quaternion * triangle.vertexB,
                quaternion * triangle.vertexC,
                triangle.notion
            );
        }

        public static Quaternion RotationTowards(Vector from, Vector to, double angle)
        {
            Vector norFrom = from.normalized, norTo = to.normalized;

            if (Vector.Angle(norTo, norFrom) > angle)
                return (new Quaternion(Math.Cos((angle * (Math.PI / 180)) / 2), norFrom * norTo * Math.Sin((angle * (Math.PI / 180)) / 2))).normalized;
            else
                return RotationOf(from, to);
        }

        public static Quaternion RotationOf(Vector from, Vector to)
        {
            Vector norFrom = from.normalized, norTo = to.normalized;
            //double angle = Vector.Angle(norTo, norFrom) * (Math.PI / 180);

            //return (new Quaternion(Math.Cos(angle / 2), norFrom * norTo * Math.Sin(angle / 2))).normalized;
            return (new Quaternion(Vector.Dot(norTo, norFrom), norFrom * norTo) / 2).normalized;
        }

        public static bool operator ==(Quaternion lhs, Quaternion rhs) => lhs.w == rhs.w && lhs.x == rhs.x && lhs.y == rhs.y && lhs.z == rhs.z;

        public static bool operator !=(Quaternion lhs, Quaternion rhs) => lhs.w != rhs.w || lhs.x != rhs.x || lhs.y != rhs.y || lhs.z != rhs.z;

        public static Vector operator *(Quaternion lhs, Vector rhs) => (lhs * new Quaternion(0, rhs) * lhs.conjugated).vectorPart;

        public static Quaternion operator *(Quaternion lhs, Quaternion rhs)
        {
            return new Quaternion(
                lhs.w * rhs.w - lhs.x * rhs.x - lhs.y * rhs.y - lhs.z * rhs.z,  // 1
                lhs.w * rhs.x + lhs.x * rhs.w + lhs.y * rhs.z - lhs.z * rhs.y,  // i
                lhs.w * rhs.y - lhs.x * rhs.z + lhs.y * rhs.w + lhs.z * rhs.x,  // j
                lhs.w * rhs.z + lhs.x * rhs.y - lhs.y * rhs.x + lhs.z * rhs.w   // k
            );
        }

        public static Quaternion operator /(Quaternion quaternion, double scaler)
        {
            return new Quaternion(quaternion.w / scaler, quaternion.x / scaler, quaternion.y / scaler, quaternion.z / scaler);
        }
        
        public static Quaternion operator *(Quaternion quaternion, double scaler)
        {
            return new Quaternion(quaternion.w * scaler, quaternion.x * scaler, quaternion.y * scaler, quaternion.z * scaler);
        }
        
        public static Quaternion operator *(double scaler, Quaternion quaternion)
        {
            return new Quaternion(quaternion.w * scaler, quaternion.x * scaler, quaternion.y * scaler, quaternion.z * scaler);
        }

        public override string ToString()
        {
            return String.Format("({0}, {1}, {2}, {3})", w, x, y, z);
        }
    }

    public struct Vector
    {
        public double x, y, z;

        public static readonly Vector zero = new Vector(0, 0, 0);
        public static readonly Vector one = new Vector(1, 1, 1);
        public static readonly Vector forward = new Vector(0, 0, 1);
        public static readonly Vector backward = new Vector(0, 0, -1);

        public Vector flatten { get => new Vector(x, y, 0); }

        public double sqrMagnitude
        {
            get { return x * x + y * y + z * z; }
        }

        public double magnitude
        {
            get { return Math.Sqrt(sqrMagnitude); }
        }

        public Vector normalized
        {
            get { return this / magnitude; }
        }

        public Vector(double x, double y, double z = 0)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public static double Angle(Vector a, Vector b)
        {
            return (180 / Math.PI) * Math.Acos(
                Dot(a, b) / Math.Sqrt(a.sqrMagnitude * b.sqrMagnitude)
            );
        }

        public static double Dot(Vector a, Vector b)
        {
            return a.x * b.x + a.y * b.y + a.z * b.z;
        }
        public static bool operator ==(Vector lhs, Vector rhs)
        {
            return lhs.x == rhs.x && lhs.y == rhs.y && lhs.z == rhs.z;
        }

        public static bool operator !=(Vector lhs, Vector rhs)
        {
            return lhs.x != rhs.x || lhs.y != rhs.y || lhs.z != rhs.z;
        }

        public static Vector operator -(Vector value)
        {
            return new Vector(-value.x, -value.y, -value.z);
        }

        public static Vector operator *(Vector lhs, Vector rhs)
        {
            return new Vector(
                lhs.y * rhs.z - lhs.z * rhs.y,
                lhs.z * rhs.x - lhs.x * rhs.z,
                lhs.x * rhs.y - lhs.y * rhs.x
            );
        }
        
        public static Vector operator +(Vector lhs, Vector rhs)
        {
            return new Vector(lhs.x + rhs.x, lhs.y + rhs.y, lhs.z + rhs.z);
        }

        public static Vector operator -(Vector lhs, Vector rhs)
        {
            return new Vector(lhs.x - rhs.x, lhs.y - rhs.y, lhs.z - rhs.z);
        }

        public static Vector operator /(Vector vector, double scaler)
        {
            return new Vector(vector.x / scaler, vector.y / scaler, vector.z / scaler);
        }

        public static Vector operator *(Vector vector, double scaler)
        {
            return new Vector(vector.x * scaler, vector.y * scaler, vector.z * scaler);
        }

        public static Vector operator *(double scaler, Vector vector)
        {
            return new Vector(vector.x * scaler, vector.y * scaler, vector.z * scaler);
        }

        public override string ToString()
        {
            return String.Format("({0}, {1}, {2})", x, y, z);
        }

        public override bool Equals(object obj)
        {
            return obj is Vector vector &&
                   x == vector.x &&
                   y == vector.y &&
                   z == vector.z;
        }

        public override int GetHashCode()
        {
            int hashCode = 1051110719;
            hashCode = hashCode * -1521134295 + x.GetHashCode();
            hashCode = hashCode * -1521134295 + y.GetHashCode();
            hashCode = hashCode * -1521134295 + z.GetHashCode();
            return hashCode;
        }
    }
}

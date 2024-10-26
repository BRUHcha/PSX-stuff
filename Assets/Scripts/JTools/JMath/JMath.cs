using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Utility class made by John Ellis, 2020
//Note: This is not perfect, it's mostly just shortcuts to the methods I already use when I make games.

public class JMath
{
    /// <summary>
    /// The amount of precision spring and lerp functions are allowed to have before being snapped to their destinations.
    /// </summary>
    public static float JEpsilon = 0.001f;

    /// <summary>
    /// Creates a spring curve that will approach 1 with the input value. Useful for emulating spring dynamics without having to use more costly methods.
    /// </summary>
    /// <param name="evaluate">The point of the curve you'd like to sample, ranging from 0 to 1.</param>
    /// <param name="exp">The approach exponent. The higher this is, the faster the spring approaches 1 and stops flipping back and forth.</param>
    /// <returns></returns>
    public static float SpringUp(float evaluate, float exp = 3f)
    {
        return (evaluate > 0f) ? ((evaluate < 1f) ? (-Mathf.Cos(evaluate * 20f) * Mathf.Pow((1f - evaluate),exp) + 1) : 1f) : 0f;
    }

    /// <summary>
    /// Creates a spring curve that will approach 0 as the input value approaches 1. Useful for emulating spring dynamics without having to use more costly methods.
    /// </summary>
    /// <param name="evaluate">The point of the curve you'd like to sample, ranging from 0 to 1.</param>
    /// <param name="exp">The approach exponent. The higher this is, the faster the spring approaches 0 and stops flipping back and forth.</param>
    /// <returns></returns>
    public static float SpringDown(float evaluate, float exp = 3f)
    {
        return 1f - ((evaluate > 0f) ? (((evaluate < 1f) ? (-Mathf.Cos(evaluate * 20f) * Mathf.Pow((1f - evaluate), exp) + 1) : 1f)) : 0f);
    }

    /// <summary>
    /// Takes a number ranging from 0 to 1 and creates a smooth equivalent going up from 0 to 1. A quick alternative to having to use bezier curves.
    /// </summary>
    /// <param name="evaluate">The point of the curve you'd like to sample, ranging from 0 to 1.</param>
    /// <returns></returns>
    public static float SmoothUp(float evaluate)
    {
        return (evaluate > 0f) ? ((evaluate < 1f) ? Mathf.Pow(evaluate, 2f) : 1f) : 0f;
    }

    /// <summary>
    /// Takes a number ranging from 0 to 1 and creates a smooth equivalent going down from 1 to 0. A quick alternative to having to use bezier curves.
    /// </summary>
    /// <param name="evaluate">The point of the curve you'd like to sample, ranging from 0 to 1.</param>
    /// <returns></returns>
    public static float SmoothDown(float evaluate)
    {
        return (evaluate > 0f) ? (((evaluate < 1f) ? Mathf.Pow(1f - evaluate, 2f) : 0f)) : 1f;
    }

    /// <summary>
    /// Takes a number ranging from 0 to 1 and creates a bulbous curved equivalent going up from 0 to 1. A quick alternative to having to use bezier curves.
    /// </summary>
    /// <param name="evaluate">The point of the curve you'd like to sample, ranging from 0 to 1.</param>
    /// <returns></returns>
    public static float CurveUp(float evaluate)
    {
        return 1f-((evaluate > 0f) ? (((evaluate < 1f) ? Mathf.Pow(1f-evaluate, 2f) : 0f)) : 1f);
    }

    /// <summary>
    /// Takes a number ranging from 1 to 0 and creates a bulbous curved equivalent going down from 1 to 0. A quick alternative to having to use bezier curves.
    /// </summary>
    /// <param name="evaluate">The point of the curve you'd like to sample, ranging from 0 to 1.</param>
    /// <returns></returns>
    public static float CurveDown(float evaluate)
    {
        return 1f - ((evaluate > 0f) ? (((evaluate < 1f) ? Mathf.Pow(evaluate, 2f) : 1f)) : 0f);
    }


    /// <summary>
    /// Generates an S-curve ranging from 0 to 1. Good stuff.
    /// </summary>
    /// <param name="evaluate"></param>
    /// <returns></returns>
    public static float SigmoidUp(float evaluate)
    {
        return (evaluate <= 0f) ? 0f : ((evaluate >= 1f) ? 1f : (1f / (1f + Mathf.Pow(2.718f, 1.35f * (-4 * Mathf.PI * evaluate + Mathf.PI * 2)))));
    }

    /// <summary>
    /// Generates an S-curve ranging from 1 to 0.
    /// </summary>
    /// <param name="evaluate"></param>
    /// <returns></returns>
    public static float SigmoidDown(float evaluate)
    {
        return (evaluate <= 0f) ? 1f : ((evaluate >= 1f) ? 0f : 1f - (1f / (1f + Mathf.Pow(2.718f, 1.35f * (-4 * Mathf.PI * evaluate + Mathf.PI * 2)))));
    }


    /// <summary>
    /// Recursive Linear Interpolation, now contained in a managed form. Best use is: a = JMath.RLI(a, b, t);
    /// </summary>
    /// <param name="a">The current color.</param>
    /// <param name="b">The goal color.</param>
    /// <param name="t">The rate of approach.</param>
    /// <returns></returns>
    public static Color RLI(Color a, Color b, float t)
    {
        t = 1f - Mathf.Pow(1f - t, Time.unscaledDeltaTime * 60f);

        return ((Mathf.Abs(b.r-a.r) + Mathf.Abs(b.g - a.g) + Mathf.Abs(b.b - a.b) + Mathf.Abs(b.a - a.a))*0.25f > JEpsilon) ? (a + (b - a) * t) : b;
    }

    /// <summary>
    /// Recursive Linear Interpolation, now contained in a managed form. Best use is: a = JMath.RLI(a, b, t);
    /// </summary>
    /// <param name="a">The current vector.</param>
    /// <param name="b">The goal vector.</param>
    /// <param name="t">The rate of approach.</param>
    /// <returns></returns>
    public static Vector3 RLI(Vector3 a, Vector3 b, float t)
    {
        t = 1f - Mathf.Pow(1f - t, Time.unscaledDeltaTime * 60f);

        return (Vector3.Distance(a, b) > JEpsilon) ? (a + (b - a) * t) : b;
    }

    /// <summary>
    /// Recursive Linear Interpolation, now contained in a managed form. Best use is: a = JMath.RLI(a, b, t);
    /// </summary>
    /// <param name="a">The current vector.</param>
    /// <param name="b">The goal vector.</param>
    /// <param name="t">The rate of approach.</param>
    /// <returns></returns>
    public static Vector2 RLI(Vector2 a, Vector2 b, float t)
    {
        t = 1f - Mathf.Pow(1f - t, Time.unscaledDeltaTime * 60f);

        return (Vector2.Distance(a, b) > JEpsilon) ? (a + (b - a) * t) : b;
    }

    /// <summary>
    /// Recursive Linear Interpolation, now contained in a managed form. Best use is: a = JMath.RLI(a, b, t);
    /// </summary>
    /// <param name="a">The current value.</param>
    /// <param name="b">The goal value.</param>
    /// <param name="t">The rate of approach.</param>
    /// <returns></returns>
    public static float RLI(float a, float b, float t)
    {
        t = 1f - Mathf.Pow(1f - t, Time.unscaledDeltaTime * 60f);

        return (Mathf.Abs(a - b) > JEpsilon) ? (a + (b - a) * t) : b;
    }
    /// <summary>
    /// Scaled Recursive Linear Interpolation, now contained in a managed form. Best use is: a = JMath.RLI(a, b, t);
    /// </summary>
    /// <param name="a">The current color.</param>
    /// <param name="b">The goal color.</param>
    /// <param name="t">The rate of approach.</param>
    /// <returns></returns>
    public static Color SRLI(Color a, Color b, float t)
    {
        t = 1f - Mathf.Pow(1f - t, Time.deltaTime * 60f);

        return ((Mathf.Abs(b.r-a.r) + Mathf.Abs(b.g - a.g) + Mathf.Abs(b.b - a.b) + Mathf.Abs(b.a - a.a))*0.25f > JEpsilon) ? (a + (b - a) * t) : b;
    }

    /// <summary>
    /// Scaled Recursive Linear Interpolation, now contained in a managed form. Best use is: a = JMath.RLI(a, b, t);
    /// </summary>
    /// <param name="a">The current vector.</param>
    /// <param name="b">The goal vector.</param>
    /// <param name="t">The rate of approach.</param>
    /// <returns></returns>
    public static Vector3 SRLI(Vector3 a, Vector3 b, float t)
    {
        t = 1f - Mathf.Pow(1f - t, Time.deltaTime * 60f);

        return (Vector3.Distance(a, b) > JEpsilon) ? (a + (b - a) * t) : b;
    }

    /// <summary>
    /// Scaled Recursive Linear Interpolation, now contained in a managed form. Best use is: a = JMath.RLI(a, b, t);
    /// </summary>
    /// <param name="a">The current vector.</param>
    /// <param name="b">The goal vector.</param>
    /// <param name="t">The rate of approach.</param>
    /// <returns></returns>
    public static Vector2 SRLI(Vector2 a, Vector2 b, float t)
    {
        t = 1f - Mathf.Pow(1f - t, Time.deltaTime * 60f);

        return (Vector2.Distance(a, b) > JEpsilon) ? (a + (b - a) * t) : b;
    }

    /// <summary>
    /// Scaled Recursive Linear Interpolation, now contained in a managed form. Best use is: a = JMath.RLI(a, b, t);
    /// </summary>
    /// <param name="a">The current value.</param>
    /// <param name="b">The goal value.</param>
    /// <param name="t">The rate of approach.</param>
    /// <returns></returns>
    public static float SRLI(float a, float b, float t)
    {
        t = 1f - Mathf.Pow(1f - t, Time.deltaTime * 60f);

        return (Mathf.Abs(a - b) > JEpsilon) ? (a + (b - a) * t) : b;
    }

    /// <summary>
    /// A centered version of perlin noise.
    /// </summary>
    /// <param name="x">Sample point x.</param>
    /// <param name="y">Sample point y.</param>
    /// <returns>A perlin value between -1f and 1f.</returns>
    public static float CPerlin(float x, float y)
    {
        return (Mathf.PerlinNoise(x,y)-0.5f)*2f;
    }

    /// <summary>
    /// A Vector2 equivalent of perlin noise.
    /// </summary>
    /// <param name="x">Sample point x.</param>
    /// <param name="y">Sample point y.</param>
    /// <returns>A vector2 with xyz coordinates ranging from -1f to 1f.</returns>
    public static Vector2 CPerlin2(float x, float y)
    {
        return new Vector2(((Mathf.PerlinNoise(x, y) - 0.5f) * 2f), ((Mathf.PerlinNoise(x, y) - 0.5f) * 2f));
    }

    /// <summary>
    /// A Vector2 equivalent of perlin noise. Uses an offset value to add variety to the different axes.
    /// </summary>
    /// <param name="x">Sample point x.</param>
    /// <param name="y">Sample point y.</param>
    /// <param name="offset">How much the different axes should vary.</param>
    /// <returns>A vector2 with xyz coordinates ranging from -1f to 1f.</returns>
    public static Vector2 CPerlin2(float x, float y, float offset)
    {
        return new Vector2(((Mathf.PerlinNoise(x, y) - 0.5f) * 2f), ((Mathf.PerlinNoise(x + offset, y + offset) - 0.5f) * 2f));
    }

    /// <summary>
    /// A Vector3 equivalent of perlin noise.
    /// </summary>
    /// <param name="x">Sample point x.</param>
    /// <param name="y">Sample point y.</param>
    /// <returns>A vector3 with xyz coordinates ranging from -1f to 1f.</returns>
    public static Vector3 CPerlin3(float x, float y)
    {
        return new Vector3(((Mathf.PerlinNoise(x, y) - 0.5f) * 2f), ((Mathf.PerlinNoise(x, y) - 0.5f) * 2f), ((Mathf.PerlinNoise(x, y) - 0.5f) * 2f));
    }

    /// <summary>
    /// A Vector3 equivalent of perlin noise. Uses an offset value to add variety to the different axes.
    /// </summary>
    /// <param name="x">Sample point x.</param>
    /// <param name="y">Sample point y.</param>
    /// <param name="offset">How much the different axes should vary.</param>
    /// <returns>A vector3 with xyz coordinates ranging from -1f to 1f.</returns>
    public static Vector3 CPerlin3(float x, float y, float offset)
    {
        return new Vector3(((Mathf.PerlinNoise(x, y) - 0.5f) * 2f), ((Mathf.PerlinNoise(x+offset, y + offset) - 0.5f) * 2f), ((Mathf.PerlinNoise(x + offset*2f, y + offset*2f) - 0.5f) * 2f));
    }

    /// <summary>
	/// Generates a vector2 with random values. Good for a variety of things.
	/// </summary>
	/// <param name="minimum">The lowest part of the range.</param>
	/// <param name="maximum">The highest part of the range.</param>
	/// <returns>Returns with a randomized Vector2.</returns>
	public static Vector2 RandomVector2(float minimum, float maximum)
    {
        return new Vector2(Random.Range(minimum, maximum), Random.Range(minimum, maximum));
    }

    /// <summary>
    /// Generates a vector2 with random integer values. Good for a variety of things.
    /// </summary>
    /// <param name="minimum">The lowest part of the range.</param>
    /// <param name="maximum">The highest part of the range.</param>
    /// <returns>Returns with a randomized Vector2.</returns>
    public static Vector2 RandomVector2(int minimum, int maximum)
    {
        return new Vector2(Random.Range(minimum, maximum), Random.Range(minimum, maximum));
    }

    /// <summary>
    /// Generates a vector3 with random values. Good for a variety of things.
    /// </summary>
    /// <param name="minimum">The lowest part of the range.</param>
    /// <param name="maximum">The highest part of the range.</param>
    /// <returns>Returns with a randomized Vector3.</returns>
    public static Vector3 RandomVector3(float minimum, float maximum)
    {
        return new Vector3(Random.Range(minimum, maximum), Random.Range(minimum, maximum), Random.Range(minimum, maximum));
    }

    /// <summary>
    /// Generates a vector3 with random integer values. Good for a variety of things.
    /// </summary>
    /// <param name="minimum">The lowest part of the range.</param>
    /// <param name="maximum">The highest part of the range.</param>
    /// <returns>Returns with a randomized Vector3.</returns>
    public static Vector3 RandomVector3(int minimum, int maximum)
    {
        return new Vector3(Random.Range(minimum, maximum), Random.Range(minimum, maximum), Random.Range(minimum, maximum));
    }

    /// <summary>
    /// Takes a value from 0 - 1 and converts it into decibels. Specifically important when working with Unity's audio mixer system.
    /// </summary>
    /// <param name="value">A float ranging from 0 - 1 that you would like to convert into Decibel format.</param>
    /// <returns></returns>
    public static float LinearToDecibel(float value)
    {
        return (value != 0f) ? (20f * Mathf.Log10(value)) : -144f;
    }

    /// <summary>
    /// Converts decibel values back into a linear percentage ranging from 0 - 1. Decibel values exceeding this might go over the upper limit, so feel free to clamp this if you really have to.
    /// </summary>
    /// <param name="db">Decibel value to convert back into a linear range between 0 - 1.</param>
    /// <returns></returns>
    public static float DecibelToLinear(float db)
    {
        return (db <= -144f) ? 0f : Mathf.Pow(10f, db / 20f);
    }

    /// <summary>
    /// Runs some basic math to create a bezier curve from some specified points.
    /// </summary>
    /// <param name="value">A value from 0 - 1 that defines how far into the bezier curve to sample.</param>
    /// <param name="points">A list of points composing the bezier curve. Must be larger than 1.</param>
    /// <returns></returns>
    public static Vector3 ComputeBezier(float value, params Vector3[] points)
    {
        if (points.Length > 2) //Bezier implementation
        {
            Vector3[] m_temp;
            Vector3[] m_Coords;

            m_Coords = points;
            
            while (m_Coords.Length > 2)
            {
                m_temp = new Vector3[m_Coords.Length - 1];

                for (int i = 0; i < m_Coords.Length - 1; i++)
                {
                    m_temp[i] = (Vector3.Lerp(m_Coords[i], m_Coords[i + 1], value));
                }

                m_Coords = m_temp;
            }

            return Vector3.Lerp(m_Coords[0], m_Coords[1], value);
        }
        else
        {
            if (points.Length == 2)
                return points[0] + (points[1] - points[0]) * value; //Just return regular linear interpolation if we only have 2 points to work from (need 3 to create a bezier curve).
            else
                return Vector3.zero;
        }
    }

    /// <summary>
    /// Runs some basic math to create a bezier curve from some specified points.
    /// </summary>
    /// <param name="value">A value from 0 - 1 that defines how far into the bezier curve to sample.</param>
    /// <param name="points">A list of points composing the bezier curve. Must be larger than 1.</param>
    /// <returns></returns>
    public static Vector2 ComputeBezier(float value, params Vector2[] points)
    {
        if (points.Length > 2) //Bezier implementation
        {
            Vector2[] m_temp;
            Vector2[] m_Coords;

            m_Coords = points;

            while (m_Coords.Length > 2)
            {
                m_temp = new Vector2[m_Coords.Length - 1];

                for (int i = 0; i < m_Coords.Length - 1; i++)
                {
                    m_temp[i] = (Vector2.Lerp(m_Coords[i], m_Coords[i + 1], value));
                }

                m_Coords = m_temp;
            }

            return Vector2.Lerp(m_Coords[0], m_Coords[1], value);
        }
        else
        {
            if (points.Length == 2)
                return points[0] + (points[1] - points[0]) * value; //Just return regular linear interpolation if we only have 2 points to work from (need 3 to create a bezier curve).
            else
                return Vector2.zero;
        }
    }

    /// <summary>
    /// Runs some basic math to create a bezier curve from some specified points.
    /// </summary>
    /// <param name="value">A value from 0 - 1 that defines how far into the bezier curve to sample.</param>
    /// <param name="points">A list of points composing the bezier curve. Must be larger than 1.</param>
    /// <returns></returns>
    public static Vector3 ComputeBezier(float value, params Transform[] points)
    {
        if (points.Length > 2) //Bezier implementation
        {
            Vector3[] m_temp;
            Vector3[] m_Coords = new Vector3[points.Length];

            for (int i = 0; i < points.Length; i++)
                m_Coords[i] = points[i].position;

            while (m_Coords.Length > 2)
            {
                m_temp = new Vector3[m_Coords.Length - 1];

                for (int i = 0; i < m_Coords.Length - 1; i++)
                {
                    m_temp[i] = (Vector3.Lerp(m_Coords[i], m_Coords[i + 1], value));
                }

                m_Coords = m_temp;
            }

            return Vector3.Lerp(m_Coords[0], m_Coords[1], value);
        }
        else
        {
            if (points.Length == 2)
                return points[0].position + (points[1].position - points[0].position) * value; //Just return regular linear interpolation if we only have 2 points to work from (need 3 to create a bezier curve).
            else
                return Vector3.zero;
        }
    }

}


//A Class for simulating a spring float.
[System.Serializable]
public class JSpring1D
{
    public float goal;
    public float tensorA = 0.4f;
    public float tensorB = 0.2f;

    public float current;

    float tracer;
    bool initialized = false;

    /// <summary>
    /// A class that helps compact spring dynamics down into a manageable format.
    /// </summary>
    /// <param name="goalValue">The position the current value will try to spring towards.</param>
    /// <param name="currentPosition">The position for the spring to start in, before it tries to reach the goal.</param>
    public JSpring1D(float goalValue, float currentPosition)
    {
        goal = goalValue;
        current = currentPosition;
        tracer = current;
        tensorA = 0.4f;
        tensorB = 0.2f;
    }

    /// <summary>
    /// A class that helps compact spring dynamics down into a manageable format.
    /// </summary>
    /// <param name="goalValue">The position the current value will try to spring towards.</param>
    /// <param name="currentPosition">The position for the spring to start in, before it tries to reach the goal.</param>
    /// <param name="a">The first tensor parameter, which controls the adjustment speed of the tracer towards the goal.</param>
    /// <param name="b">The second tensor parameter, which controls how fast the current position tracks to the tracer.</param>
    public JSpring1D(float goalValue, float currentPosition, float a = 0.4f, float b = 0.2f)
    {
        goal = goalValue;
        current = currentPosition;
        tracer = current;
        tensorA = a;
        tensorB = b;
    }

    public float Update()
    {
        float tensarA = 1f - Mathf.Pow(1f - tensorA, Time.unscaledDeltaTime * 60f);
        float tensarB = 1f - Mathf.Pow(1f - tensorB, Time.unscaledDeltaTime * 60f);


        if (!initialized)
        {
            tracer = current;
            initialized = true;
        }

        if (Mathf.Abs(tracer - goal) + Mathf.Abs(tracer - current) < JMath.JEpsilon)
            current = goal;
        else
        {
            tracer += (goal - current) * tensarA;
            current += (tracer - current) * tensarB;
        }

        return current;
    }
}

//A Class for simulating spring dynamics in 2D.
[System.Serializable]
public class JSpring2D
{
    public Vector2 goal;
    public float tensorA = 0.4f;
    public float tensorB = 0.2f;

    public Vector2 current;

    Vector2 tracer;
    bool initialized = false;

    /// <summary>
    /// A class that helps compact spring dynamics down into a manageable format.
    /// </summary>
    /// <param name="goalValue">The position the current value will try to spring towards.</param>
    /// <param name="currentPosition">The position for the spring to start in, before it tries to reach the goal.</param>
    public JSpring2D(Vector2 goalValue, Vector2 currentPosition)
    {
        goal = goalValue;
        current = currentPosition;
        tracer = current;
        tensorA = 0.4f;
        tensorB = 0.2f;
    }

    /// <summary>
    /// A class that helps compact spring dynamics down into a manageable format.
    /// </summary>
    /// <param name="goalValue">The position the current value will try to spring towards.</param>
    /// <param name="currentPosition">The position for the spring to start in, before it tries to reach the goal.</param>
    /// <param name="a">The first tensor parameter, which controls the adjustment speed of the tracer towards the goal.</param>
    /// <param name="b">The second tensor parameter, which controls how fast the current position tracks to the tracer.</param>
    public JSpring2D(Vector2 goalValue, Vector2 currentPosition, float a = 0.4f, float b = 0.2f)
    {
        goal = goalValue;
        current = currentPosition;
        tracer = current;
        tensorA = a;
        tensorB = b;
    }

    public Vector2 Update()
    {
        float tensarA = 1f - Mathf.Pow(1f - tensorA, Time.unscaledDeltaTime * 60f);
        float tensarB = 1f - Mathf.Pow(1f - tensorB, Time.unscaledDeltaTime * 60f);

        if (!initialized)
        {
            tracer = current;
            initialized = true;
        }

        if (Vector2.Distance(tracer, goal) + Vector2.Distance(tracer, current) < JMath.JEpsilon)
            current = goal;
        else
        {
            tracer += (goal - current) * tensarA;
            current += (tracer - current) * tensarB;

        }

        return current;
    }
}

//A Class for simulating spring dynamics in 3D.
[System.Serializable]
public class JSpring3D
{
    public Vector3 goal;
    public float tensorA = 0.4f;
    public float tensorB = 0.2f;

    public Vector3 current;

    Vector3 tracer;
    bool initialized = false;

    /// <summary>
    /// A class that helps compact spring dynamics down into a manageable format.
    /// </summary>
    /// <param name="goalValue">The position the current value will try to spring towards.</param>
    /// <param name="currentPosition">The position for the spring to start in, before it tries to reach the goal.</param>
    public JSpring3D(Vector3 goalValue, Vector3 currentPosition)
    {
        goal = goalValue;
        current = currentPosition;
        tracer = current;
        tensorA = 0.4f;
        tensorB = 0.2f;
    }

    /// <summary>
    /// A class that helps compact spring dynamics down into a manageable format.
    /// </summary>
    /// <param name="goalValue">The position the current value will try to spring towards.</param>
    /// <param name="currentPosition">The position for the spring to start in, before it tries to reach the goal.</param>
    /// <param name="a">The first tensor parameter, which controls the adjustment speed of the tracer towards the goal.</param>
    /// <param name="b">The second tensor parameter, which controls how fast the current position tracks to the tracer.</param>
    public JSpring3D(Vector3 goalValue, Vector3 currentPosition, float a = 0.4f, float b = 0.2f)
    {
        goal = goalValue;
        current = currentPosition;
        tracer = current;
        tensorA = a;
        tensorB = b;
    }

    public Vector3 Update()
    {
        float tensarA = 1f - Mathf.Pow(1f - tensorA, Time.unscaledDeltaTime * 60f);
        float tensarB = 1f - Mathf.Pow(1f - tensorB, Time.unscaledDeltaTime * 60f);

        if (!initialized)
        {
            tracer = current;
            initialized = true;
        }

        if (Vector3.Distance(tracer,goal) + Vector3.Distance(tracer, current) < JMath.JEpsilon)
            current = goal;
        else
        {
            tracer += (goal - current) * tensarA;
            current += (tracer - current) * tensarB;
        }

        return current;
    }
}
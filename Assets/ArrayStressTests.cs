using UnityEngine;
using Unity.Jobs;
using Unity.Collections;

public class ArrayStressTests : MonoBehaviour
{
    struct VelocityJobParallelFor : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<Vector3> velocity;
        public NativeArray<Vector3> position;

        public float deltaTime;

        public void Execute(int i)
        {
            position[i] = position[i] + velocity[i] * deltaTime;
        }
    }

	struct VelocityIJob : IJob
    {
        [ReadOnly]
        public NativeArray<Vector3> velocity;
        public NativeArray<Vector3> position;

        public float deltaTime;

        public void Execute()
        {
            for (int i = 0; i < position.Length; i++)
            {
                position[i] = position[i] + velocity[i] * deltaTime;
            }
        }
    }

    struct VelocityNonjobNativeArray
    {
        [ReadOnly]
        public NativeArray<Vector3> velocity;
        public NativeArray<Vector3> position;

        public float deltaTime;

        public void Execute(int i)
        {
            position[i] = position[i] + velocity[i] * deltaTime;
        }
    }

    struct VelocityNonjobBuildIn
    {
        [ReadOnly]
        public Vector3[] velocity;
        public Vector3[] position;

        public float deltaTime;

        public void Execute(int i)
        {
            position[i] = position[i] + velocity[i] * deltaTime;
        }
    }

    NativeArray<Vector3> positionNative;
    NativeArray<Vector3> velocityNative;

    Vector3[] positionBuildIn;
    Vector3[] velocityBuildIn;

	public int n = 100000;

    void Start()
    {
        positionNative = new NativeArray<Vector3>(n, Allocator.Persistent);
        velocityNative = new NativeArray<Vector3>(n, Allocator.Persistent);

        positionBuildIn = new Vector3[n];
        velocityBuildIn = new Vector3[n];

        for (var i = 0; i < velocityNative.Length; i++)
        {
            velocityNative[i] = new Vector3(0, 10, 0);
            velocityBuildIn[i] = new Vector3(0, 10, 0);
        }
    }

    int jobMode = 1;
    void Update()
    {
        if (jobMode == 1)
        {
            UpdateJobParallelFor();
        }
		if (jobMode == 2)
        {
            UpdateIJob();
        }
        else if (jobMode == 3)
        {
            UpdateNativeArrayFromStruct();
        }
        else if (jobMode == 4)
        {
            UpdateBuildInArrayFromStruct();
        }
		else if (jobMode == 5)
        {
            UpdateNativeArrayInline();
        }
        else if (jobMode == 6)
        {
            UpdateBuildInArrayInline();
        }


        if (Input.GetKeyDown(KeyCode.A))
        {
            jobMode = 1;
            Debug.Log("jobMode " + jobMode);
        }
		else if (Input.GetKeyDown(KeyCode.B))
        {
            jobMode = 2;
            Debug.Log("jobMode " + jobMode);
        }
        else if (Input.GetKeyDown(KeyCode.C))
        {
            jobMode = 3;
            Debug.Log("jobMode " + jobMode);
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            jobMode = 4;
            Debug.Log("jobMode " + jobMode);
        }
		else if (Input.GetKeyDown(KeyCode.E))
        {
            jobMode = 5;
            Debug.Log("jobMode " + jobMode);
        }
        else if (Input.GetKeyDown(KeyCode.F))
        {
            jobMode = 6;
            Debug.Log("jobMode " + jobMode);
        }
    }

    void UpdateJobParallelFor()
    {
        int processorCount = System.Environment.ProcessorCount;

        var job = new VelocityJobParallelFor
        {
            deltaTime = Time.deltaTime,
            position = positionNative,
            velocity = velocityNative
        };

        JobHandle jobHandle = job.Schedule(positionNative.Length, processorCount);

        jobHandle.Complete();
    }

	void UpdateIJob()
    {
        var job = new VelocityIJob
        {
            deltaTime = Time.deltaTime,
            position = positionNative,
            velocity = velocityNative
        };

        JobHandle jobHandle = job.Schedule();

        jobHandle.Complete();
    }

    void UpdateNativeArrayFromStruct()
    {
        var job = new VelocityNonjobNativeArray
        {
            deltaTime = Time.deltaTime,
            position = positionNative,
            velocity = velocityNative
        };

        for (int i = 0; i < positionNative.Length; i++)
        {
            job.Execute(i);
        }
    }

    void UpdateBuildInArrayFromStruct()
    {
        var job = new VelocityNonjobBuildIn
        {
            deltaTime = Time.deltaTime,
            position = positionBuildIn,
            velocity = velocityBuildIn
        };

        for (int i = 0; i < positionBuildIn.Length; i++)
        {
            job.Execute(i);
        }
    }

	void UpdateNativeArrayInline()
    {
        float deltaTime = Time.deltaTime;

        for (int i = 0; i < positionNative.Length; i++)
        {
            positionNative[i] = positionNative[i] + velocityNative[i] * deltaTime;
        }
    }

    void UpdateBuildInArrayInline()
    {
        float deltaTime = Time.deltaTime;

        for (int i = 0; i < positionNative.Length; i++)
        {
            positionBuildIn[i] = positionBuildIn[i] + velocityBuildIn[i] * deltaTime;
        }
    }

    void OnApplicationQuit()
    {
        positionNative.Dispose();
        velocityNative.Dispose();
    }
}

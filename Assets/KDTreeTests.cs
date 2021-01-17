// This test class allows to test performance for kdtree when running
// 1 : single threaded with regular arrays
// 2 : single threaded with native arrays
// 3 : jobified (multithreaded) with native arrays

using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;

public class KDTreeTests : MonoBehaviour
{
    Vector3[] pointsBuildIn;
    NativeArray<float3> pointsNative;
    NativeArray<float3> queries;

    NativeArray<int> answers;

    public int nData = 5000;
    public int nQueries = 5000;
    public int seed = 48;

    public bool calculateDistancesSumOnUpdate = false;

    struct KdSearchJob : IJobParallelFor
    {
        [ReadOnly] public KDTreeStruct kdJob;
        [ReadOnly] public NativeArray<float3> pointsJob;
        [ReadOnly] public NativeArray<float3> queriesJob;

        public NativeArray<int> answersJob;

        public void Execute(int i)
        {
            answersJob[i] = kdJob.FindNearest(queriesJob[i]);
        }
    }

    [BurstCompile]
    struct KdSearchJobBursted : IJobParallelFor
    {
        [ReadOnly] public KDTreeStruct kdJob;
        [ReadOnly] public NativeArray<float3> pointsJob;
        [ReadOnly] public NativeArray<float3> queriesJob;

        public NativeArray<int> answersJob;

        public void Execute(int i)
        {
            answersJob[i] = kdJob.FindNearest(queriesJob[i]);
        }
    }

    void Start()
    {
        UnityEngine.Random.InitState(seed);

        pointsBuildIn = new Vector3[nData];

        for (int i = 0; i < pointsBuildIn.Length; i++)
        {
            pointsBuildIn[i] = UnityEngine.Random.insideUnitSphere;
        }

        pointsNative = new NativeArray<float3>(nData, Allocator.Persistent);

        for (int i = 0; i < pointsBuildIn.Length; i++)
        {
            pointsNative[i] = pointsBuildIn[i];
        }

        queries = new NativeArray<float3>(nQueries, Allocator.Persistent);
        answers = new NativeArray<int>(nQueries, Allocator.Persistent);

        for (int i = 0; i < queries.Length; i++)
        {
            queries[i] = UnityEngine.Random.insideUnitSphere;
        }

        CorrectnessTest();
    }

    void CorrectnessTest()
    {
        float regularSum = NeighbourDistancesSumRegular();
        float nativeSum = NeighbourDistancesSumNative();

        Debug.Log($"Regular KDTree sum: {regularSum}, Native KDTree sum: {nativeSum}");
    }

    KDTree kdTreeRegular;
    float NeighbourDistancesSumRegular()
    {
        kdTreeRegular = KDTree.MakeFromPoints(pointsBuildIn);

        float neighbourDistancesSum = 0f;

        for (int i = 0; i < queries.Length; i++)
        {
            int id = kdTreeRegular.FindNearest(queries[i]);
            neighbourDistancesSum = neighbourDistancesSum + math.length(queries[i] - pointsNative[id]);
        }

        return neighbourDistancesSum;
    }

    KDTreeStruct kdTreeNative;
    float NeighbourDistancesSumNative()
    {
        kdTreeNative = new KDTreeStruct();
        kdTreeNative.MakeFromPoints(pointsNative);

        float neighbourDistancesSum = 0f;

        for (int i = 0; i < queries.Length; i++)
        {
            int id = kdTreeNative.FindNearest(queries[i]);
            neighbourDistancesSum = neighbourDistancesSum + math.length(queries[i] - pointsNative[id]);
        }

        return neighbourDistancesSum;
    }

    int imode = 0;
    void Update()
    {
        float neighbourDistancesSum = 0f;

        if (imode == 0)
        {
            for (int i = 0; i < queries.Length; i++)
            {
                int j = kdTreeRegular.FindNearest(queries[i]);

                if(calculateDistancesSumOnUpdate)
                {
                    neighbourDistancesSum = neighbourDistancesSum + math.length(queries[i] - pointsNative[j]);
                }
            }
        }
        else if (imode == 1)
        {
            for (int i = 0; i < queries.Length; i++)
            {
                int j = kdTreeNative.FindNearest(queries[i]);

                if(calculateDistancesSumOnUpdate)
                {
                    neighbourDistancesSum = neighbourDistancesSum + math.length(queries[i] - pointsNative[j]);
                }
            }
        }
        else if (imode == 2)
        {

            int processorCount = System.Environment.ProcessorCount;

            new KdSearchJob
            {
                kdJob = kdTreeNative,
                pointsJob = pointsNative,
                queriesJob = queries,
                answersJob = answers
            }.Schedule(queries.Length, processorCount).Complete();

            for (int i = 0; i < answers.Length; i++)
            {
                int j = answers[i];

                if(calculateDistancesSumOnUpdate)
                {
                    neighbourDistancesSum = neighbourDistancesSum + math.length(queries[i] - pointsNative[j]);
                }
            }
        }
        else if (imode == 3)
        {
            int processorCount = System.Environment.ProcessorCount;

            new KdSearchJobBursted
            {
                kdJob = kdTreeNative,
                pointsJob = pointsNative,
                queriesJob = queries,
                answersJob = answers
            }.Schedule(queries.Length, processorCount).Complete();

            for (int i = 0; i < answers.Length; i++)
            {
                int j = answers[i];

                if(calculateDistancesSumOnUpdate)
                {
                    neighbourDistancesSum = neighbourDistancesSum + math.length(queries[i] - pointsNative[j]);
                }
            }
        }

        if (calculateDistancesSumOnUpdate)
        {
            Debug.Log(neighbourDistancesSum);
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            imode = 0;
            Debug.Log("Running with regular arrays");
        }
        else if (Input.GetKeyDown(KeyCode.B))
        {
            imode = 1;
            Debug.Log("Running with native arrays");
        }
        else if (Input.GetKeyDown(KeyCode.C))
        {
            imode = 2;
            Debug.Log("Running jobified");
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            imode = 3;
            Debug.Log("Running jobified with burst");
        }
    }

    void OnApplicationQuit()
    {
        kdTreeNative.DisposeArrays();
        pointsNative.Dispose();
        queries.Dispose();
        answers.Dispose();
    }
}

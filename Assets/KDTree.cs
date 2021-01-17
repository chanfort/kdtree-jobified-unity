// KDTree.cs - A Stark, September 2009.

//	This class implements a data structure that stores a list of points in space.
//	A common task in game programming is to take a supplied point and discover which
//	of a stored set of points is nearest to it. For example, in path-plotting, it is often
//	useful to know which waypoint is nearest to the player's current
//	position. The kd-tree allows this "nearest neighbour" search to be carried out quickly,
//	or at least much more quickly than a simple linear search through the list.

//	At present, the class only allows for construction (using the MakeFromPoints static method)
//	and nearest-neighbour searching (using FindNearest). More exotic kd-trees are possible, and
//	this class may be extended in the future if there seems to be a need.

//	The nearest-neighbour search returns an integer index - it is assumed that the original
//	array of points is available for the lifetime of the tree, and the index refers to that
//	array.

using UnityEngine;
using System.Collections.Generic;

public class KDTree
{    public KDTree[] lr;
    public Vector3 pivot;
    public int pivotIndex;
    public int axis;

    //	Change this value to 2 if you only need two-dimensional X,Y points. The search will
    //	be quicker in two dimensions.
    const int numDims = 3;

    public KDTree()
    {
        lr = new KDTree[2];
    }

    public static KDTree MakeFromPointsGo(List<GameObject> pointsGo)
    {
        Vector3[] pos = new Vector3[pointsGo.Count];

        for (int i = 0; i < pointsGo.Count; i++)
        {
            pos[i] = pointsGo[i].transform.position;
        }

        KDTree kd = MakeFromPoints(pos);
        return kd;
    }

    //	Make a new tree from a list of points.
    public static KDTree MakeFromPoints(params Vector3[] points)
    {
        int[] indices = Iota(points.Length);
        return MakeFromPointsInner(0, 0, points.Length - 1, points, indices);
    }

    //	Recursively build a tree by separating points at plane boundaries.
    static KDTree MakeFromPointsInner(
		int depth,
		int stIndex, int enIndex,
		Vector3[] points,
		int[] inds
	)
    {
        KDTree root = new KDTree();
        root.axis = depth % numDims;
        int splitPoint = FindPivotIndex(points, inds, stIndex, enIndex, root.axis);

        root.pivotIndex = inds[splitPoint];
        root.pivot = points[root.pivotIndex];

        int leftEndIndex = splitPoint - 1;

        if (leftEndIndex >= stIndex)
        {
            root.lr[0] = MakeFromPointsInner(depth + 1, stIndex, leftEndIndex, points, inds);
        }

        int rightStartIndex = splitPoint + 1;

        if (rightStartIndex <= enIndex)
        {
            root.lr[1] = MakeFromPointsInner(depth + 1, rightStartIndex, enIndex, points, inds);
        }

        return root;
    }

    static void SwapElements(int[] arr, int a, int b)
    {
        int temp = arr[a];
        arr[a] = arr[b];
        arr[b] = temp;
    }

    //	Simple "median of three" heuristic to find a reasonable splitting plane.
    static int FindSplitPoint(Vector3[] points, int[] inds, int stIndex, int enIndex, int axis)
    {
        float a = points[inds[stIndex]][axis];
        float b = points[inds[enIndex]][axis];
        int midIndex = (stIndex + enIndex) / 2;
        float m = points[inds[midIndex]][axis];

        if (a > b)
        {
            if (m > a)
            {
                return stIndex;
            }

            if (b > m)
            {
                return enIndex;
            }

            return midIndex;
        }
        else
        {
            if (a > m)
            {
                return stIndex;
            }

            if (m > b)
            {
                return enIndex;
            }

            return midIndex;
        }
    }

    //	Find a new pivot index from the range by splitting the points that fall either side
    //	of its plane.
    public static int FindPivotIndex(Vector3[] points, int[] inds, int stIndex, int enIndex, int axis)
    {
        int splitPoint = FindSplitPoint(points, inds, stIndex, enIndex, axis);

        Vector3 pivot = points[inds[splitPoint]];
        SwapElements(inds, stIndex, splitPoint);

        int currPt = stIndex + 1;
        int endPt = enIndex;

        while (currPt <= endPt)
        {
            Vector3 curr = points[inds[currPt]];

            if ((curr[axis] > pivot[axis]))
            {
                SwapElements(inds, currPt, endPt);
                endPt--;
            }
            else
            {
                SwapElements(inds, currPt - 1, currPt);
                currPt++;
            }
        }

        return currPt - 1;
    }

    public static int[] Iota(int num)
    {
        int[] result = new int[num];

        for (int i = 0; i < num; i++)
        {
            result[i] = i;
        }

        return result;
    }

    //	Find the nearest point in the set to the supplied point.
    public int FindNearest(Vector3 pt)
    {
        float bestSqDist = float.MaxValue;
        int bestIndex = -1;

        Search(pt, ref bestSqDist, ref bestIndex);

        return bestIndex;
    }

    public float FindNearest_R(Vector3 pt)
    {
        float bestSqDist = float.MaxValue;
        int bestIndex = -1;

        Search(pt, ref bestSqDist, ref bestIndex);

        return (Mathf.Sqrt(bestSqDist));
    }

    //	Recursively search the tree.
    void Search(Vector3 pt, ref float bestSqSoFar, ref int bestIndex)
    {
        float mySqDist = (pivot - pt).sqrMagnitude;

        if (mySqDist < bestSqSoFar)
        {
            bestSqSoFar = mySqDist;
            bestIndex = pivotIndex;
        }

        float planeDist = pt[axis] - pivot[axis];

        int selector = planeDist <= 0 ? 0 : 1;

        if (lr[selector] != null)
        {
            lr[selector].Search(pt, ref bestSqSoFar, ref bestIndex);
        }

        selector = (selector + 1) % 2;

        float sqPlaneDist = planeDist * planeDist;

        if ((lr[selector] != null) && (bestSqSoFar > sqPlaneDist))
        {
            lr[selector].Search(pt, ref bestSqSoFar, ref bestIndex);
        }
    }

	// Find and returns	k-th nearest neighbour
    public int FindNearestK(Vector3 pt, int k)
    {
        float bestSqDist = float.MaxValue;
        float minSqDist = 0f;
        int bestIndex = -1;

        for (int i = 0; i < k - 1; i++)
        {
            SearchK(pt, ref bestSqDist, ref minSqDist, ref bestIndex);

            minSqDist = bestSqDist;
            bestSqDist = float.MaxValue;
            bestIndex = -1;
        }

        SearchK(pt, ref bestSqDist, ref minSqDist, ref bestIndex);

        return bestIndex;
    }

	// Find and returns all k neighbours
    public int[] FindNearestsK(Vector3 pt, int k)
    {
        float bestSqDist = float.MaxValue;
        float minSqDist = 0f;
        int bestIndex = -1;
        int[] bestIndexK = new int[k];

        for (int i = 0; i < k - 1; i++)
        {
            SearchK(pt, ref bestSqDist, ref minSqDist, ref bestIndex);
            bestIndexK[i] = bestIndex;

            minSqDist = bestSqDist;
            bestSqDist = float.MaxValue;
            bestIndex = -1;
        }

        SearchK(pt, ref bestSqDist, ref minSqDist, ref bestIndex);
        bestIndexK[k - 1] = bestIndex;

        return bestIndexK;
    }

	// Find and returns	k-th nearest neighbour distance
    public float FindNearestK_R(Vector3 pt, int k)
    {
        float bestSqDist = float.MaxValue;
        float minSqDist = 0f;
        int bestIndex = -1;

        for (int i = 0; i < k - 1; i++)
        {
            SearchK(pt, ref bestSqDist, ref minSqDist, ref bestIndex);

            minSqDist = bestSqDist;
            bestSqDist = float.MaxValue;
            bestIndex = -1;
        }

        SearchK(pt, ref bestSqDist, ref minSqDist, ref bestIndex);

        return (Mathf.Sqrt(bestSqDist));
    }

	// Find and returns all k neighbours distances
    public float[] FindNearestsK_R(Vector3 pt, int k)
    {
        float bestSqDist = float.MaxValue;
        float minSqDist = 0f;
        int bestIndex = -1;
        float[] bestDistances = new float[k];

        for (int i = 0; i < k - 1; i++)
        {
            SearchK(pt, ref bestSqDist, ref minSqDist, ref bestIndex);
            bestDistances[i] = Mathf.Sqrt(bestSqDist);

            minSqDist = bestSqDist;
            bestSqDist = float.MaxValue;
            bestIndex = -1;
        }

        SearchK(pt, ref bestSqDist, ref minSqDist, ref bestIndex);
        bestDistances[k - 1] = Mathf.Sqrt(bestSqDist);

        return bestDistances;
    }

    public int[] FindNearestsR(Vector3 pt, float rcrit)
    {
        int k = 2;
        
        int[] pts = null;
        if (k > 1)
        {
            pts = FindNearestsK(pt, k - 1);
        }

        return pts;
    }

    void SearchK(Vector3 pt, ref float bestSqSoFar, ref float minSqDist, ref int bestIndex)
    {
        float mySqDist = (pivot - pt).sqrMagnitude;

        if (mySqDist < bestSqSoFar)
        {
            if (mySqDist > minSqDist)
            {
                bestSqSoFar = mySqDist;
                bestIndex = pivotIndex;
            }
        }

        float planeDist = pt[axis] - pivot[axis];

        int selector = planeDist <= 0 ? 0 : 1;

        if (lr[selector] != null)
        {
            lr[selector].SearchK(pt, ref bestSqSoFar, ref minSqDist, ref bestIndex);
        }

        selector = (selector + 1) % 2;

        float sqPlaneDist = planeDist * planeDist;

        if ((lr[selector] != null) && (bestSqSoFar > sqPlaneDist))
        {
            lr[selector].SearchK(pt, ref bestSqSoFar, ref minSqDist, ref bestIndex);
        }
    }

    //	Simple output of tree structure - mainly useful for getting a rough
    //	idea of how deep the tree is (and therefore how well the splitting
    //	heuristic is performing).
    public string Dump(int level)
    {
        string result = pivotIndex.ToString().PadLeft(level) + "\n";

        if (lr[0] != null)
        {
            result += lr[0].Dump(level + 2);
        }

        if (lr[1] != null)
        {
            result += lr[1].Dump(level + 2);
        }

        return result;
    }
}

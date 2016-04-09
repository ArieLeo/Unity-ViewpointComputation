﻿/*
-----------------------------------------------------------------------------
This source file is part of ViewpointComputationLib (a viewpoint computation library)
For more info on the project, contact Roberto Ranon at roberto.ranon@uniud.it.

Copyright (c) 2013- University of Udine, Italy - http://hcilab.uniud.it
Also see acknowledgements in readme.txt
-----------------------------------------------------------------------------

 CLUtils.cs: A collection of utility classes
-----------------------------------------------------------------------------
*/

using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System;

/// <summary>
/// Static class of utility methods for CLTarget-related functions
/// </summary>    
public static class TargetUtils
{

	public static Bounds intersect( Bounds a, Bounds b)
	{
		Bounds result = new Bounds ();
		result.min = new Vector3 (Mathf.Max (a.min.x, b.min.x), Mathf.Max (a.min.y, b.min.y), Mathf.Max (a.min.z, b.min.z));
		result.max = new Vector3 (Mathf.Min (a.max.x, b.max.x), Mathf.Min (a.max.y, b.max.y), Mathf.Min (a.max.z, b.max.z));
		return result;

	}


	/// <summary>
	/// Relative positioning of the camera with respect to the AABB of the target.
	/// </summary>    
	public enum RelativePositioning
	{
		LEFT = 0,
		RIGHT = 1,
		BOTTOM  = 2,
		TOP = 3,
		FRONT = 4,
		BACK = 5
	}

	/// <summary>
	/// Axes
	/// </summary>
	public enum Axis
	{
		RIGHT = 0,
		UP = 1,
		FORWARD  = 2,
		WORLD_UP = 3
	}

	/// <summary>
	/// Hull vertex table
	/// </summary>
	private static int[,] hullVertexTable = 
	{   
		/**     Hull Vertex Table, from Schmalstieg-Tobler's work on "Real Time Bounding Box Area Computation", 
             *      the table is valid for all rectangular bounding boxes. The first six columns encode the ordered list 
             *      of vertices of the bbox which are visible from a certain viewpoint, the 7th column reports the number 
             *      of visible vertices for rapid access.
             *          In real-time a 6-bit integer is generated by performing six checks on the position of the viewpoint 
             *      with respect to the six planes of the bounding box. The i-th bit of the integer is 1 if the i-th vertex 
             *      is visible from the viewpoint, 0 if not. The integer is then used to perform a lookup in the table to 
             *      know the right sorting of visible vertices in order to compute a contour integral which yields the area 
             *      of the projected bounding box.
             *         Columns starting at 43 refer to the case in which the camera is inside the bounding box, therefore 
             *      the area is not calculated.
             * 
                    I       II      III     IV      V       VI      Number             Case                   Valid   
                    ================================================================================================= */
		{       0,      0,      0,      0,      0,      0,      0       },  // Camera inside box    

		{       0,      4,      7,      3,      0,      0,      4       },  // Left                     x
		{       1,      2,      6,      5,      0,      0,      4       },  // Right                    x

		{       0,      0,      0,      0,      0,      0,      0       },  // Camera inside box        

		{       0,      1,      5,      4,      0,      0,      4       },  // Bottom                   x
		{       0,      1,      5,      4,      7,      3,      6       },  // Bottom left              x
		{       0,      1,      2,      6,      5,      4,      6       },  // Bottom right             x

		{       0,      0,      0,      0,      0,      0,      0       },  // Camera inside box                    

		{       2,      3,      7,      6,      0,      0,      4       },  // Top                      x
		{       4,      7,      6,      2,      3,      0,      6       },  // Top left                 x
		{       2,      3,      7,      6,      5,      1,      6       },  // Top right                x

		{       0,      0,      0,      0,      0,      0,      0       },  // Camera inside box
		{       0,      0,      0,      0,      0,      0,      0       },  // Camera inside box
		{       0,      0,      0,      0,      0,      0,      0       },  // Camera inside box
		{       0,      0,      0,      0,      0,      0,      0       },  // Camera inside box
		{       0,      0,      0,      0,      0,      0,      0       },  // Camera inside box

		{       0,      3,      2,      1,      0,      0,      4       },  // Front                    x
		{       0,      4,      7,      3,      2,      1,      6       },  // Front left               x
		{       0,      3,      2,      6,      5,      1,      6       },  // Front right              x

		{       0,      0,      0,      0,      0,      0,      0       },  // Camera inside box    

		{       0,      3,      2,      1,      5,      4,      6       },  // Front bottom             x
		{       2,      1,      5,      4,      7,      3,      6       },  // Front bottom left        x
		{       0,      3,      2,      6,      5,      4,      6       },  // Front bottom right       x

		{       0,      0,      0,      0,      0,      0,      0       },  // Camera inside box    

		{       0,      3,      7,      6,      2,      1,      6       },  // Front top                x
		{       0,      4,      7,      6,      2,      1,      6       },  // Front top left           x
		{       0,      3,      7,      6,      5,      1,      6       },  // Front top right          x

		{       0,      0,      0,      0,      0,      0,      0       },  // Camera inside box
		{       0,      0,      0,      0,      0,      0,      0       },  // Camera inside box
		{       0,      0,      0,      0,      0,      0,      0       },  // Camera inside box
		{       0,      0,      0,      0,      0,      0,      0       },  // Camera inside box
		{       0,      0,      0,      0,      0,      0,      0       },  // Camera inside box

		{       4,      5,      6,      7,      0,      0,      4       },  // Back                     x
		{       4,      5,      6,      7,      3,      0,      6       },  // Back left                x
		{       1,      2,      6,      7,      4,      5,      6       },  // Back right               x

		{       0,      0,      0,      0,      0,      0,      0       },  // Camera inside box

		{       0,      1,      5,      6,      7,      4,      6       },  // Back bottom              x
		{       0,      1,      5,      6,      7,      3,      6       },  // Back bottom left         x
		{       0,      1,      2,      6,      7,      4,      6       },  // Back bottom right        x

		{       0,      0,      0,      0,      0,      0,      0       },  // Camera inside box

		{       2,      3,      7,      4,      5,      6,      6       },  // Back top                 x
		{       0,      4,      5,      6,      2,      3,      6       },  // Back top left            x
		{       1,      2,      3,      7,      4,      5,      6       },  // Back top right           x

		{       0,      0,      0,      0,      0,      0,      0,      }   // 43rd+ row                x

	};


	/// <summary>
	/// Gets the number of visible vertices from a specified position encoded with an int.
	/// </summary>
	public static int Number (int encodedCase)
	{
		if (encodedCase >= 43)
			return 0;
		return hullVertexTable [encodedCase, 6];
	}

	/// <summary>
	/// Get a vertex of the hull vertex table given the position case and the index.
	/// </summary>
	public static int Vertex (int index, int encodedCase)
	{
		if (encodedCase >= 43)
			return 0;
		return hullVertexTable [encodedCase, index];
	}


	/// <summary>
	/// Converts between vertex table vertex ordering and our BB vertex ordering
	/// </summary>
	public static Vector3 ReturnAABBVertex (int vertex, Bounds box)
	{
		switch (vertex) {
		case 0: // FAR_LEFT_BOTTOM 
			return box.min;
		case 3: // FAR_LEFT_TOP 
			return new Vector3 (box.min.x, box.max.y, box.min.z); 
		case 2: // FAR_RIGHT_TOP 
			return new Vector3 (box.max.x, box.max.y, box.min.z); 
		case 1: // FAR_RIGHT_BOTTOM 
			return new Vector3 (box.max.x, box.min.y, box.min.z); 
		case 6: // NEAR_RIGHT_TOP 
			return box.max; 
		case 7: // NEAR_LEFT_TOP 
			return new Vector3 (box.min.x, box.max.y, box.max.z); 
		case 4: // NEAR_LEFT_BOTTOM 
			return new Vector3 (box.min.x, box.min.y, box.max.z); 
		case 5: // NEAR_RIGHT_BOTTOM 
			return new Vector3 (box.max.x, box.min.y, box.max.z); 
		default:
			return box.min;
		}	

	}

	/// <summary>
	/// Computes area of a 2D convex polygon
	/// </summary>
	public static float ComputeScreenArea (List<Vector2> poly)
	{

		float sum = 0.0f;

		// Compute contour integral
		for (int i = 0; i < poly.Count; i++) {
			// Sum (without division)
			sum += (poly [i].x - poly [(i + 1) % poly.Count].x) *
				(poly [i].y + poly [(i + 1) % poly.Count].y);
		}

		/** Return divided sum (part of the contour integral formula, which is done
             *  at the end for performance reasons), could be done in the for loop */
		// negative because order is clockwise (don't know why)

		return -sum * 0.5f;  
	}


	/// <summary>
	/// Clips a polygon using an axis, return true if the whole polygon is clipped
	/// </summary>
	public static bool ClipSide2D (float p, bool clipGreater, int axis, List<Vector2> inPoly, List<Vector2> outPoly)
	{

		outPoly.Clear ();
		int i0 = -1;

		Vector2 pt1 = new Vector2 ();
		bool c1 = true;

		float negate = clipGreater ? -1 : 1;

		// Find a point that is not clipped
		for (i0 = 0; (i0 < inPoly.Count) && c1; ++i0) {
			pt1 = inPoly [i0];       
			c1 = (negate * pt1 [axis]) < (negate * p);
		}

		// We incremented i0 one time too many
		--i0;

		if (c1) {
			// We could not find an unclipped point
			return true;
		}

		outPoly.Add (pt1);

		// for each point in inPoly,
		//     if the point is outside the side and the previous one was also outside, continue
		//     if the point is outside the side and the previous one was inside, cut the line
		//     if the point is inside the side and the previous one was also inside, append the points
		//     if the point is inside the side and the previous one was outside, cut the line    
		for (int i = 1; i <= inPoly.Count; ++i) {
			Vector2 pt2 = inPoly [(i + i0) % inPoly.Count];
			bool c2 = (negate * pt2 [axis]) < (negate * p);

			if (c1 ^ c2) {

				if (!c1 && c2 && (i > 1)) {
					// Unclipped to clipped trasition and not the first iteration
					outPoly.Add (pt1);
				}

				// only one point is clipped, find where the line crosses the clipping plane


				float alpha;
				if (pt2 [axis] == pt1 [axis]) {
					alpha = 0;
				} else {
					alpha = (p - pt1 [axis]) / (pt2 [axis] - pt1 [axis]);
				}
				outPoly.Add (Vector2.Lerp (pt1, pt2, alpha));
			} else if (! (c1 || c2) && (i != 1)) {
				// neither point is clipped (don't do this the first time 
				// because we appended the first pt before the loop)
				outPoly.Add (pt1);
			}

			pt1 = pt2;
			c1 = c2;
		}

		return false;
	}

	/// <summary>
	/// Clips a 2D polygon by a Rect
	/// </summary>
	public static void Clip (Rectangle clipRect, List<Vector2> inPoly, List<Vector2> outPoly)
	{

		bool greaterThan = true;
		bool lessThan = false;
		int X = 0;
		int Y = 1;

		List<Vector2> temp = new List<Vector2> (10);

		bool entirelyClipped =
			ClipSide2D (clipRect.xMin, lessThan, X, inPoly, temp) ||
			ClipSide2D (clipRect.xMax, greaterThan, X, temp, outPoly) ||
			ClipSide2D (clipRect.yMin, lessThan, Y, outPoly, temp) || 
			ClipSide2D (clipRect.yMax, greaterThan, Y, temp, outPoly);

		if (entirelyClipped) {
			outPoly.Clear ();
		}
	}


	/// <summary>
	/// Computes a point along a linear spline
	/// </summary>
	public static float LinearSpline (float x, List<float> controlX, List<float> controlY)
	{
		// Off the beginning
		if ((controlX.Count == 1) || (x < controlX [0])) {
			return controlY [0];
		}

		for (int i = 1; i < controlX.Count; ++i) {
			if (x < controlX [i]) {
				float alpha = (controlX [i] - x) / (controlX [i] - controlX [i - 1]);
				return controlY [i] * (1 - alpha) + controlY [i - 1] * alpha;
			}
		}

		// Off the end
		return controlY [controlX.Count - 1];	

	}



}

/// <summary>
/// We define our own rectangle class since Unity Rect has the y coordinate downwards
/// </summary>
public class Rectangle
{

	/// <summary>
	/// Extreme points of the Rectangle
	/// </summary>    
	public float xMin, xMax, yMin, yMax;

	/// <summary>
	/// Constructor.
	/// </summary>    
	public Rectangle (float _xMin, float _xMax, float _yMin, float _yMax)
	{
		xMin = _xMin;
		xMax = _xMax;
		yMin = _yMin;
		yMax = _yMax;

	}

	/// <summary>
	/// Returns area of the rectangle.
	/// </summary>    
	public float CalculateArea ()
	{
		return (xMax - xMin) * (yMax - yMin);	

	}

	/// <summary>
	/// Returns width of the rectangle
	/// </summary>    
	public float CalculateWidth ()
	{
		return 	(xMax - xMin);
	}

	/// <summary>
	/// Returns height of the rectangle.
	/// </summary>    
	public float CalculateHeight ()
	{
		return (yMax - yMin);	
	}



}

/// <summary>
/// Class to represent an hierarchical clustering of VC problem solutions
/// </summary>
public class CandidateCluster
{
	// best candidate of the cluster
	public CLCandidate bestCandidate;

	// children clusters (empty for leaves)
	public List<CandidateCluster> children = new List<CandidateCluster>();

	// parent cluster (null for root)
	public CandidateCluster parent;

	// max distance between members of the cluster
	public float maxDistance; 

	// constructor, also sets the best candidate
	public CandidateCluster(CLCandidate c)
	{
		bestCandidate = c;
	}
	
	
	public void Traverse(Action<CLCandidate> action)
	{
		action(bestCandidate);
		foreach (var child in children)
			child.Traverse(action);
	}


	// returns all candidates in the cluster (leaf nodes of the subtree where this is root)
	public List<CLCandidate> GetCandidatesInCluster() {

		List<CLCandidate> result = new List<CLCandidate> ();
		if (children.Count == 0) {
			result.Add (bestCandidate);
		} else {
			foreach ( CandidateCluster c in children ) {
				result.AddRange( c.GetCandidatesInCluster() );
			}
		}

		return result;

	}

	// returns all candidates that are n levels below the current one; 
	// if n = 0 -> return this candidate
	// if n = 1 -> return this candidate children
	// if n = 2 -> etc.
	public List<CLCandidate> GetCandidatesAtLevel( int level ) {

		List<CLCandidate> result = new List<CLCandidate> ();
		if ( (level == 0) || (children.Count == 0)) {
			result.Add (bestCandidate);
		} else {
			foreach ( CandidateCluster c in children ) {
				result.AddRange( c.GetCandidatesAtLevel( level - 1 ) );
			}
		}
		
		return result;

	}


	// returns the distance between this cluster and another one
	// we use a complete link approach for hierarchical clustering,
	// so this is the maximum distance between two members of each
	// cluster
	public float Distance( CandidateCluster other, bool useBestPosition ) {
		
		// get leaf nodes of this cluster
		List<CLCandidate> leaves = GetCandidatesInCluster ();

		// get leaf nodes of other cluster
		List<CLCandidate> otherLeaves = other.GetCandidatesInCluster ();

		// compute max distance
		float distance = 0.0f;

		foreach (CLCandidate c1 in leaves) {
			foreach (CLCandidate c2 in otherLeaves ) {
				float d = c1.Distance( c2, useBestPosition );
				if (d > distance ) {
					distance = d;
				}
			}
		}

		return distance;
	}
}


/// <summary>
/// A clustering of candidates. We use agglomerative hierarchical clustering.
/// </summary>
public class HierarchicalClustering {


	// the cluster, implemented as a tree of CLCandidates
	public CandidateCluster clustering;


	/// <summary>
	/// Initializes a new instance of the <see cref="CandidatesClustering"/> class and performs clusterng
	/// </summary>
	/// <param name="solver">Solver, whose found solutions we want to cluster</param>
	/// <param name="minSat">Minimum sat. We will exclude solutions that do not reach this sat.</param>
	/// <param name="maxDistance">Max distance.</param>
	/// <param name="maxClusters">Max clusters. Maximum number of clusters to build</param>
	public HierarchicalClustering( CLSolver solver, float minSat, bool useBestPosition ) {

		List<CandidateCluster> clusters = new List<CandidateCluster> ();
		
		// select only candidates with sat >= minSat and assign each candidate to a cluster
		for (int i=0; i < solver.numberOfCandidates; i++) {
			if ( solver.candidates[i].bestEvaluation >= minSat ) {
				clusters.Add ( new CandidateCluster( solver.candidates[i] ));
			}
		}

		Debug.Log ("found " + clusters.Count + " candidates with min sat");

		// now, loop until |clusters| = 1
		while (clusters.Count > 1) {

			// compute distances matrix
			float[,] distances = ComputeDistanceMatrix (clusters, useBestPosition );
			
			// find closest clusters
			int cluster1=0;
			int cluster2=0;
			float minDistance = Mathf.Infinity;
			for (int i=0; i<clusters.Count; i++) {
				for (int j=0; j<clusters.Count; j++) {
					if ( i > j) { // we need to compute distance
						if ( distances[i,j] < minDistance ) {
							cluster1 = i;
							cluster2 = j;
							minDistance = distances[i,j];
							
						}
					}
				}
			}
			
			float value1, value2;
			// select representative
			if ( useBestPosition ) {
				value1 = clusters[cluster1].bestCandidate.bestEvaluation;
				value2 = clusters[cluster2].bestCandidate.bestEvaluation;
			} else {
				value1 = clusters[cluster1].bestCandidate.evaluation;
				value2 = clusters[cluster2].bestCandidate.evaluation;
			}


			int best = (value1 > value2)? cluster1:cluster2;
			CandidateCluster newCluster = new CandidateCluster(clusters[best].bestCandidate);
			newCluster.maxDistance = distances[cluster1,cluster2];
			newCluster.children.Add ( clusters[cluster1]);
			newCluster.children.Add ( clusters[cluster2]);
			clusters[cluster1].parent = newCluster;
			clusters[cluster2].parent = newCluster;
			clusters.Add ( newCluster );

			
			// remove clusters cluster1 and cluster2
			clusters.RemoveAt( cluster1 );
			clusters.RemoveAt( cluster2 );

		}

		if (clusters.Count > 0) {

			clustering = clusters [0];

		}	

	}

	// given a list of clusters, computes the distances matrix
	private float[,] ComputeDistanceMatrix (List<CandidateCluster> clusters, bool useBestPosition) {

		float[,] distances = new float[ clusters.Count, clusters.Count ];
		for (int i=0; i<clusters.Count; i++) {
			for (int j=0; j<clusters.Count; j++) {
				distances [i, j] = -1.0f;
			}
		}
		
		for (int i=0; i<clusters.Count; i++) {
			for (int j=0; j<clusters.Count; j++) {
				if ( distances[i,j] < 0.0f) { // we need to compute distance
					if ( i==j ) {
						distances[i,j] = Mathf.Infinity;
					} 
					else {
						float d = clusters[i].Distance( clusters[j], useBestPosition );
						distances[i,j] = d;
						distances[j,i] = d;
						
					}
					
				}
				
			}
		}

		return distances;
	}

}


/// <summary>
/// Abstract class for a satisfaction function
/// </summary>
public abstract class CLSatFunction {

	/// <summary>
	/// Computes the satisfaction in [0,1], given a value
	/// </summary>
	/// <returns>The satisfaction.</returns>
	/// <param name="value">Value.</param>
	public abstract float ComputeSatisfaction (float value);

	/// <summary>
	/// Builds the cumulative distribution function.
	/// </summary>
	public abstract void BuildCDF ();

	/// <summary>
	/// Generates a random point in the given domain, with distribution according to the sat function, 
	/// i.e. with more probability where satisfaction is higher
	/// </summary>
	/// <returns>The random X point.</returns>
	public abstract float GenerateRandomXPoint ();

	/// <summary>
	/// The domain of the sat function
	/// </summary>
	public Vector2 domain;

}


/// <summary>
/// Defines a satisfaction function with a linear spline
/// </summary>
public class CLLinearSplineSatFunction : CLSatFunction {

	/// The linear spline satisfaction function (x values)
	public List<float> satXPoints;

	/// The linear spline satisfaction function (y values)
	public List<float> satYPoints;

	/// The cumulative satisfaction function, see http://en.wikipedia.org/wiki/Cumulative_distribution_function (x points)
	public List<float> cumulativeSatXPoints;

	/// The cumulative satisfaction function, see http://en.wikipedia.org/wiki/Cumulative_distribution_function (y points)
	public List<float> cumulativeSatYPoints;

	/// <summary>
	/// Computes the satisfaction in [0,1], given a value
	/// </summary>
	/// <returns>The satisfaction.</returns>
	/// <param name="value">Value.</param>
	public override float ComputeSatisfaction (float value)
	{
		return TargetUtils.LinearSpline (value, satXPoints, satYPoints);
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="CLLinearSplineSatFunction"/> class.
	/// </summary>
	/// <param name="_satXPoints">Sat X points.</param>
	/// <param name="_satYPoints">Sat Y points.</param>
	public CLLinearSplineSatFunction ( ) {}

	/// <summary>
	/// Initializes a new instance of the <see cref="CLLinearSplineSatFunction"/> class.
	/// </summary>
	/// <param name="_satXPoints">Sat X points.</param>
	/// <param name="_satYPoints">Sat Y points.</param>
	public CLLinearSplineSatFunction ( List<float> _satXPoints, List<float> _satYPoints ) {

		satXPoints = new List<float> (_satXPoints);
		satYPoints = new List<float> (_satYPoints);
		domain = new Vector2 (_satXPoints [0], _satXPoints [_satXPoints.Count - 1]);
		BuildCDF ();
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="CLLinearSplineSatFunction"/> class.
	/// </summary>
	/// <param name="min_range">Minimum range.</param>
	/// <param name="max_range">Max range.</param>
	/// <param name="desired_value">Desired value.</param>
	/// <param name="deviation_percentage">Deviation percentage.</param>
	/// <param name="increasing">If set to <c>true</c> increasing.</param>
	/// <param name="degrade_gracefully">If set to <c>true</c> degrade gracefully.</param>
	public CLLinearSplineSatFunction (float min_range, float max_range, float desired_value, float deviation_percentage, bool increasing, bool degrade_gracefully = false)
	{

		List<float> satXPoints = new List<float> ();
		List<float> satYPoints = new List<float> ();
		int points = 0;
		float range = Mathf.Abs (max_range - min_range);
		float epsilon = range * 0.001f;

		points++;

		if ((desired_value - (range * deviation_percentage + epsilon)) > min_range) {
			satXPoints.Add (min_range);
			satYPoints.Add (0.0f);
			satXPoints.Add (desired_value - (range * deviation_percentage + epsilon));
			satYPoints.Add (0.05f);
			points++;
			satXPoints.Add (desired_value - (range * deviation_percentage));
			satYPoints.Add (0.8f);
			points++;
		} else {
			if (desired_value > min_range) {
				satXPoints.Add (min_range);
				satYPoints.Add (0.8f);

			}	
		}
		if (desired_value < max_range) {
			satXPoints.Add (desired_value);
			satYPoints.Add (1.0f);
			points++;
			if (increasing)
			{
				satXPoints.Add (max_range);
				satYPoints.Add ( 1.0f);
			}
			else if ((desired_value + (range * deviation_percentage) + epsilon) < max_range) {
				satXPoints.Add (desired_value + (range * deviation_percentage));
				satYPoints.Add (0.8f);
				points++;
				satXPoints.Add (desired_value + (range * deviation_percentage) + epsilon);
				satYPoints.Add (0.05f);
				points++;
				satXPoints.Add (max_range);
				satYPoints.Add (0.0f);
			} else {
				satXPoints.Add (max_range);
				satYPoints.Add (0.8f);
				points++;

			}
		} else {
			satXPoints.Add (max_range);
			satYPoints.Add (1.0f);
			points++;

		}

		domain = new Vector2 (satXPoints [0], satXPoints [satXPoints.Count - 1]);
		BuildCDF ();

	}


	/// <summary>
	/// Builds the cumulative distribution function.
	/// </summary>
	public override void BuildCDF() {

		cumulativeSatXPoints = new List<float> (satXPoints);

		// besides setting the points of the sat spline, we also compute the "cumulative distribution function", which
		// we will use to generate random points according to how satifaction is distributed.

		cumulativeSatYPoints = new List<float> (satXPoints.Count);
		cumulativeSatYPoints.Add (0.0f);

		for (int i=1; i<satXPoints.Count; i++) {
			float width = Mathf.Abs (satXPoints [i] - satXPoints [i - 1]);
			float height = (satYPoints [i] + satYPoints [i - 1]);
			float area = width * height * 0.5f;
			cumulativeSatYPoints.Add (area + cumulativeSatYPoints [i - 1]);
		}
	}


	public override float GenerateRandomXPoint ()
	{
		float Y_point = UnityEngine.Random.Range (0.0f, cumulativeSatYPoints [cumulativeSatYPoints.Count - 1]);

		// computes the value x such that cumulative_sat(x) = Y_point.
		return TargetUtils.LinearSpline (Y_point, cumulativeSatYPoints, cumulativeSatXPoints);	
	}




}



public class CLGaussianSatFunction : CLLinearSplineSatFunction {

	/// <summary>
	/// value at which the function returns 1.0
	/// </summary>
	public float u;

	/// <summary>
	/// standard deviation
	/// </summary>
	public float sigma;

	public CLGaussianSatFunction( float _u, float _sigma, Vector2 _domain ) {
		u = _u;
		sigma = _sigma;
		domain = _domain;

		// computes linear spline approximation of gaussian function
		satXPoints = new List<float> ();
		satYPoints = new List<float> ();
		float increment = (domain.y - domain.x) / 9.0f;

		for (int i = 0; i < 10; i++) {
			float x_val = domain.x + i * increment;
			satXPoints.Add (x_val);
			satYPoints.Add (ComputeGaussian (x_val));
			if ((x_val < u) && ((x_val + increment) > u)) {  // if u is between actual and next x_val
				satXPoints.Add (u);
				satYPoints.Add (1.0f);

			}
		}
		BuildCDF ();




	}

	/// <summary>
	/// Computes the satisfaction in [0,1], given a value
	/// </summary>
	/// <returns>The satisfaction.</returns>
	/// <param name="value">Value.</param>
	public float ComputeGaussian (float value) {

		// restrict value to domain
		if ( value > domain.y ) { value = domain.y;   }
		if ( value < domain.x ) { value = domain.x;   }

		return Mathf.Exp (-0.5f * (Mathf.Pow (value - u, 2.0f)) / (sigma * sigma));
	}
		
}






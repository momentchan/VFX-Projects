#ifndef __POSE_DATA_INCLUDE__
#define __POSE_DATA_INCLUDE__
struct PoseData {
	float score;
	float2 center;
	float2 extent;
	float2 keyPoints[4]; // hip center, full body ROI, sholder center, upper body ROI
};
#endif
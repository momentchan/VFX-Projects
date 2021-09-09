#ifndef __POSE_REGION_INCLUDE__
#define __POSE_REGION_INCLUDE__
struct PoseRegion {
	// float4(center_x, center_y, size, angle)
	float4 box;
	// delta box
	float4 dBox;
	// image crop matrix
	float4x4 cropMatrix;
};
#endif
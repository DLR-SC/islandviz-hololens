float distanceFromPoint(float3 referencePoint, float3 evaluatingPoint)
{
	float distance = length(referencePoint - evaluatingPoint);
	return distance;
}

//discard drawing of a point in the world if it is outside of a defined circle
void circleClip(float3 inputWorld, float3 referencePosition, float3 referenceNormal, float referenceRadius) 
{


	float3 vertWorld = inputWorld.xyz;
	float3 vertRef = vertWorld - referencePosition;
	float distToPlane = dot(vertRef, referenceNormal);
	float3 vertProj = vertWorld - distToPlane * referenceNormal;
	float distProj = distance(referencePosition, vertProj);

	clip(distProj > 1);

}

#ifndef WOBBLE_CONCEPT_SKY_INCLUDED
#define WOBBLE_CONCEPT_SKY_INCLUDED
float WobbleCloud(float2 p,float2 centre)
{
    float2 q=(p-centre)/float2(.34,.085);
    float base=1-smoothstep(.55,1,dot(q,q));
    float2 a=(p-centre-float2(-.11,.05))/float2(.15,.095);
    float2 b=(p-centre-float2(.08,.075))/float2(.19,.12);
    return max(base,max(1-smoothstep(.5,1,dot(a,a)),1-smoothstep(.5,1,dot(b,b))));
}
float3 WobbleSky(float3 d,float3 top,float3 horizon,float3 bottom,float3 accent,float strength)
{
    float y=d.y;
    float band=exp(-pow((y+.45)*2.8,2));
    float3 sky=lerp(lerp(bottom,top,smoothstep(-1,-.2,y)),horizon,band*.6);
    float2 p=float2(atan2(d.z,d.x),y);
    float cloud=max(WobbleCloud(p,float2(-2.1,-.22)),max(WobbleCloud(p,float2(-.8,-.46)),WobbleCloud(p,float2(1.5,-.28))));
    sky=lerp(sky,lerp(horizon,bottom,.5),cloud*.48);
    float veil=smoothstep(.55,.95,sin(d.x*3+d.z*2+_Time.y*.025)*.5+.5)*exp(-pow((d.y+.4)*5,2));
    return lerp(sky,accent,veil*strength);
}
#endif

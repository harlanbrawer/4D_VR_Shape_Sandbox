using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Single;

public class Helpers : MonoBehaviour
{

    //obsolete class because unity has vector 4 class, keeping for reference or future use
    public static Vector4 makeVec4(Matrix<float> mat)
    {
        return new Vector4(mat[0, 0], mat[1, 0], mat[2, 0], mat[3, 0]);
    }

    //helper matrix class for (now deleted) hypercube 4-object testing
    public static Matrix<float> toMat(Vector4 vec)
    {
        return DenseMatrix.OfArray(new float[,] {
            {vec.x},
            {vec.y},
            {vec.z},
            {vec.w}
        });
    }

    //obsolete class because unity has vector 3 class, keeping for reference or future use
    public static Vector3 project(Vector4 vec)
    {
        float distance = 2f;
        float w = 1 / (distance - vec.w);
        float[,] projectionArr = new float[,] {
            {w, 0, 0, 0},
            {0, w, 0, 0},
            {0, 0, w, 0}
        };
        return new Vector3(vec.x * w, vec.y * w, vec.z * w);
    }

    //rotates the supplied vecter4 by the given angle pairs. Each pair represnets a "2 coordinate rotation"
    //which changes the two coordinates at the same time as if it were being rotated with respect to an axis
    //that only affects those two coordinates.
    //This rotation was invented by extrapolating a 3d rotation matrix to 4d
    //what is show below in fullRotArr is an array representation of all 6 rotation matricies multiplied together.
    //this was done so that the matrix multiplication would not have to be done every frame, thus speeding up the rotation calculation.
    public static void rotate(ref Vector4 vec, float ang_xy = 0, float ang_yz = 0, float ang_xz = 0, float ang_xw = 0, float ang_yw = 0, float ang_zw = 0)
    {

        float cos_ang_xy = Mathf.Cos(ang_xy);
        float cos_ang_xz = Mathf.Cos(ang_xz);
        float cos_ang_xw = Mathf.Cos(ang_xw);
        float cos_ang_yz = Mathf.Cos(ang_yz);
        float cos_ang_yw = Mathf.Cos(ang_yw);
        float cos_ang_zw = Mathf.Cos(ang_zw);

        float sin_ang_xy = Mathf.Sin(ang_xy);
        float sin_ang_xz = Mathf.Sin(ang_xz);
        float sin_ang_xw = Mathf.Sin(ang_xw);
        float sin_ang_yz = Mathf.Sin(ang_yz);
        float sin_ang_yw = Mathf.Sin(ang_yw);
        float sin_ang_zw = Mathf.Sin(ang_zw);

        float[,] fullRotArr = new float[,]
            {
                {cos_ang_xw * (cos_ang_xy * cos_ang_xz +sin_ang_xy * sin_ang_xz * sin_ang_yz), -cos_ang_yw * cos_ang_yz * sin_ang_xy -sin_ang_xw
                * sin_ang_yw * (cos_ang_xy * cos_ang_xz + sin_ang_xy * sin_ang_xz * sin_ang_yz), cos_ang_zw * (-cos_ang_xy * sin_ang_xz +
                cos_ang_xz * sin_ang_xy * sin_ang_yz) + (cos_ang_yz * sin_ang_xy * sin_ang_yw - cos_ang_yw * sin_ang_xw * (cos_ang_xy *
                cos_ang_xz +sin_ang_xy * sin_ang_xz * sin_ang_yz)) * sin_ang_zw, cos_ang_zw * (cos_ang_yz * sin_ang_xy * sin_ang_yw -
                cos_ang_yw * sin_ang_xw * (cos_ang_xy * cos_ang_xz +sin_ang_xy * sin_ang_xz * sin_ang_yz)) + (cos_ang_xy * sin_ang_xz -
                cos_ang_xz * sin_ang_xy * sin_ang_yz) * sin_ang_zw},

                {cos_ang_xw * (cos_ang_xz * sin_ang_xy -cos_ang_xy * sin_ang_xz *
                sin_ang_yz), -cos_ang_xz * sin_ang_xw * sin_ang_xy * sin_ang_yw +cos_ang_xy * (cos_ang_yw * cos_ang_yz + sin_ang_xw *
                sin_ang_xz * sin_ang_yw * sin_ang_yz), -cos_ang_zw * (sin_ang_xy * sin_ang_xz + cos_ang_xy * cos_ang_xz * sin_ang_yz) -
                (cos_ang_xz * cos_ang_yw * sin_ang_xw * sin_ang_xy + cos_ang_xy * (cos_ang_yz * sin_ang_yw -cos_ang_yw * sin_ang_xw *
                sin_ang_xz * sin_ang_yz)) * sin_ang_zw, cos_ang_xy * cos_ang_zw * (-cos_ang_yz * sin_ang_yw + cos_ang_yw * sin_ang_xw *
                sin_ang_xz * sin_ang_yz) +sin_ang_xy * sin_ang_xz * sin_ang_zw +cos_ang_xz * (-cos_ang_yw * cos_ang_zw * sin_ang_xw *
                sin_ang_xy + cos_ang_xy * sin_ang_yz * sin_ang_zw)},

                {cos_ang_xw * cos_ang_yz * sin_ang_xz, -cos_ang_yz * sin_ang_xw *
                sin_ang_xz * sin_ang_yw +cos_ang_yw * sin_ang_yz, cos_ang_xz * cos_ang_yz * cos_ang_zw - (cos_ang_yw * cos_ang_yz *
                sin_ang_xw * sin_ang_xz + sin_ang_yw * sin_ang_yz) * sin_ang_zw, -cos_ang_yw * cos_ang_yz * cos_ang_zw * sin_ang_xw *
                sin_ang_xz -cos_ang_zw * sin_ang_yw * sin_ang_yz -cos_ang_xz * cos_ang_yz * sin_ang_zw},
                
                {sin_ang_xw, cos_ang_xw * sin_ang_yw, cos_ang_xw * cos_ang_yw * sin_ang_zw, cos_ang_xw * cos_ang_yw * cos_ang_zw}
            };

        //this part takes the rotation array, turns it into a matrix, and applies it to a coordinate vector producing the rotated vector.
        Matrix<float> toRot = DenseMatrix.OfArray(fullRotArr);
        float[,] coords = {
            {vec.x},
            {vec.y},
            {vec.z},
            {vec.w}
        };
        Matrix<float> coordMat = DenseMatrix.OfArray(coords);
        Matrix<float> rotated = toRot.Multiply(coordMat);

        vec = makeVec4(rotated);
    }


    public float x, y, z, w;
}

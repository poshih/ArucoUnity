﻿using ArucoUnity.Objects;
using ArucoUnity.Plugin;

namespace ArucoUnity
{
  /// \addtogroup aruco_unity_package
  /// \{

  namespace Objects.Trackers
  {
    public class ArucoGridBoardTracker : ArucoObjectTracker
    {
      // ArucoObjectTracker methods

      public override void Detect(int cameraId, Aruco.Dictionary dictionary, Cv.Mat image)
      {
        base.Detect(cameraId, dictionary, image);

        ArucoMarkerTracker markerTracker = arucoTracker.MarkerTracker;

        if (arucoTracker.RefineDetectedMarkers && arucoTracker.MarkerTracker.DetectedMarkers[cameraId][dictionary] > 0)
        {
          foreach (var arucoBoard in arucoTracker.GetArucoObjects<ArucoGridBoard>(dictionary))
          {
            Aruco.RefineDetectedMarkers(image, arucoBoard.Board, markerTracker.MarkerCorners[cameraId][dictionary],
              markerTracker.MarkerIds[cameraId][dictionary], markerTracker.RejectedCandidateCorners[cameraId][dictionary]);
            markerTracker.DetectedMarkers[cameraId][dictionary] = (int)markerTracker.MarkerIds[cameraId][dictionary].Size();
          }
        }
      }

      public override void Draw(int cameraId, Aruco.Dictionary dictionary, Cv.Mat image)
      {
        base.Draw(cameraId, dictionary, image);

        foreach (var arucoGridBoard in arucoTracker.GetArucoObjects<ArucoGridBoard>(dictionary))
        {
          if (arucoTracker.DrawAxes && undistortion != null && arucoGridBoard.Rvec != null)
          {
            Aruco.DrawAxis(image, undistortion.RectifiedCameraMatrices[cameraId], undistortion.UndistortedDistCoeffs[cameraId],
              arucoGridBoard.Rvec, arucoGridBoard.Tvec, arucoGridBoard.AxisLength);
          }
        }
      }

      public override void EstimateTransforms(int cameraId, Aruco.Dictionary dictionary)
      {
        base.EstimateTransforms(cameraId, dictionary);

        foreach (var arucoGridBoard in arucoTracker.GetArucoObjects<ArucoGridBoard>(dictionary))
        {
          Cv.Vec3d rvec = null, tvec = null;
          int markersUsedForEstimation = 0;

          if (arucoTracker.MarkerTracker.DetectedMarkers[cameraId][dictionary] > 0 && undistortion != null)
          {
            markersUsedForEstimation = Aruco.EstimatePoseBoard(arucoTracker.MarkerTracker.MarkerCorners[cameraId][dictionary],
              arucoTracker.MarkerTracker.MarkerIds[cameraId][dictionary], arucoGridBoard.Board, undistortion.RectifiedCameraMatrices[cameraId],
              undistortion.UndistortedDistCoeffs[cameraId], out rvec, out tvec);
          }

          arucoGridBoard.Rvec = rvec;
          arucoGridBoard.Tvec = tvec;
          arucoGridBoard.MarkersUsedForEstimation = markersUsedForEstimation;
        }
      }

      public override void UpdateTransforms(int cameraId, Aruco.Dictionary dictionary)
      {
        base.UpdateTransforms(cameraId, dictionary);

        // Update transform of each tracked board
        foreach (var arucoGridBoard in arucoTracker.GetArucoObjects<ArucoGridBoard>(dictionary))
        {
          if (arucoGridBoard.Rvec != null)
          {
            UpdateTransform(arucoGridBoard, arucoGridBoard.Rvec, arucoGridBoard.Tvec, cameraId);

            // Adjust the estimated coordinates
            arucoGridBoard.transform.localPosition 
              += arucoGridBoard.transform.right * arucoGridBoard.transform.localScale.x / 2
              + arucoGridBoard.transform.forward * arucoGridBoard.transform.localScale.z / 2;
          }
        }
      }
    }
  }

  /// \} aruco_unity_package
}
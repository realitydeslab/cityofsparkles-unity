
using UnityEngine;
using System.Collections;
using System;

namespace AmberGarage.Trajen
{
    public static class GeodeticConverter
    {
        // Geodetic system parameters
        private const double kSemimajorAxis = 6378137;
        private const double kSemiminorAxis = 6356752.3142;
        private const double kFirstEccentricitySquared = 6.69437999014 * 0.001;
        private const double kSecondEccentricitySquared = 6.73949674228 * 0.001;
        private const double kFlattening = 1 / 298.257223563;

        private static bool isValidGeodetic(double lat, double lng)
        {
            return -90 <= lat && lat <= 90 && -180 <= lng && lng <= 180;
        }

        private static double rad2Deg(double radians)
        {
            return (radians / Math.PI) * 180.0;
        }

        private static double deg2Rad(double degrees)
        {
            return (degrees / 180.0) * Math.PI;
        }

        private static void geodetic_to_ecef(double latitude, double longitude, double altitude,
                                              out double x, out double y, out double z)
        {
            // Convert geodetic coordinates to ECEF.
            // http://code.google.com/p/pysatel/source/browse/trunk/coord.py?r=22
            double lat_rad = deg2Rad(latitude);
            double lon_rad = deg2Rad(longitude);
            double xi = Math.Sqrt(1 - kFirstEccentricitySquared * Math.Sin(lat_rad) * Math.Sin(lat_rad));
            x = (kSemimajorAxis / xi + altitude) * Math.Cos(lat_rad) * Math.Cos(lon_rad);
            y = (kSemimajorAxis / xi + altitude) * Math.Cos(lat_rad) * Math.Sin(lon_rad);
            z = (kSemimajorAxis / xi * (1 - kFirstEccentricitySquared) + altitude) * Math.Sin(lat_rad);
        }

        private static double cbrt(double x)
        {
            return Math.Pow(x, 1.0 / 3.0);
        }

        private static void ecef_to_geodetic(double x, double y, double z,
                                             out double latitude, out double longitude, out double altitude)
        {
            // Convert ECEF coordinates to geodetic coordinates.
            // J. Zhu, "Conversion of Earth-centered Earth-fixed coordinates
            // to geodetic coordinates," IEEE Transactions on Aerospace and
            // Electronic Systems, vol. 30, pp. 957-961, 1994.

            double r = Math.Sqrt(x * x + y * y);
            double Esq = kSemimajorAxis * kSemimajorAxis - kSemiminorAxis * kSemiminorAxis;
            double F = 54 * kSemiminorAxis * kSemiminorAxis * z * z;
            double G = r * r + (1 - kFirstEccentricitySquared) * z * z - kFirstEccentricitySquared * Esq;
            double C = (kFirstEccentricitySquared * kFirstEccentricitySquared * F * r * r) / Math.Pow(G, 3);
            double S = cbrt(1 + C + Math.Sqrt(C * C + 2 * C));
            double P = F / (3 * Math.Pow((S + 1 / S + 1), 2) * G * G);
            double Q = Math.Sqrt(1 + 2 * kFirstEccentricitySquared * kFirstEccentricitySquared * P);
            double r_0 = -(P * kFirstEccentricitySquared * r) / (1 + Q)
                            + Math.Sqrt(
                                         0.5 * kSemimajorAxis * kSemimajorAxis * (1 + 1.0 / Q)
                                        - P * (1 - kFirstEccentricitySquared) * z * z / (Q * (1 + Q)) - 0.5 * P * r * r);
            double U = Math.Sqrt(Math.Pow((r - kFirstEccentricitySquared * r_0), 2) + z * z);
            double V = Math.Sqrt(Math.Pow((r - kFirstEccentricitySquared * r_0), 2) + (1 - kFirstEccentricitySquared) * z * z);
            double Z_0 = kSemiminorAxis * kSemiminorAxis * z / (kSemimajorAxis * V);
            altitude = U * (1 - kSemiminorAxis * kSemiminorAxis / (kSemimajorAxis * V));
            latitude = rad2Deg(Math.Atan((z + kSecondEccentricitySquared * Z_0) / r));
            longitude = rad2Deg(Math.Atan2(y, x));
        }

        private static void ecef_to_ned(double x, double y, double z,
                                        double ref_latitude, double ref_longitude, double ref_altitude,
                                        out double north, out double east, out double down)
        {
            // Converts ECEF coordinate position into local-tangent-plane NED.
            // Coordinates relative to given ECEF coordinate frame.
            double ref_latitude_rad = deg2Rad(ref_latitude);
            double ref_longitude_rad = deg2Rad(ref_longitude);
            double ref_ecef_x, ref_ecef_y, ref_ecef_z;
            geodetic_to_ecef(ref_latitude, ref_longitude, ref_altitude, out ref_ecef_x, out ref_ecef_y, out ref_ecef_z);
            double dx = x - ref_ecef_x, dy = y - ref_ecef_y, dz = z - ref_ecef_z;

            double phiP = Math.Atan2(ref_ecef_z, Math.Sqrt(Math.Pow(ref_ecef_x, 2) + Math.Pow(ref_ecef_y, 2)));

            double sLat = Math.Sin(phiP), cLat = Math.Cos(phiP);
            double sLon = Math.Sin(ref_longitude_rad), cLon = Math.Cos(ref_longitude_rad);
            north = -sLat * cLon * dx - sLat * sLon * dy + cLat * dz;
            east = -sLon * dx + cLon * dy;
            down = -cLat * cLon * dx - cLat * sLon * dy - sLat * dz;
        }

        private static void ned_to_ecef(double north, double east, double down,
                                        double ref_latitude, double ref_longitude, double ref_altitude,
                                        out double x, out double y, out double z)
        {
            // NED (north/east/down) to ECEF coordinates
            double ref_latitude_rad = deg2Rad(ref_latitude);
            double ref_longitude_rad = deg2Rad(ref_longitude);
            double ref_ecef_x, ref_ecef_y, ref_ecef_z;
            geodetic_to_ecef(ref_latitude, ref_longitude, ref_altitude, out ref_ecef_x, out ref_ecef_y, out ref_ecef_z);

            double sLat = Math.Sin(ref_latitude_rad), cLat = Math.Cos(ref_latitude_rad);
            double sLon = Math.Sin(ref_longitude_rad), cLon = Math.Cos(ref_longitude_rad);

            x = -sLat * cLon * north - sLon * east - cLat * cLon * down + ref_ecef_x;
            y = -sLat * sLon * north + cLon * east - cLat * sLon * down + ref_ecef_y;
            z = cLat * north - sLat * down + ref_ecef_z;
        }

        public static void geodetic_to_ned(double latitude, double longitude, double altitude,
                                     double ref_latitude, double ref_longitude, double ref_altitude,
                                    out double north, out double east, out double down)
        {
            // Geodetic position to local NED frame
            double x, y, z;
            geodetic_to_ecef(latitude, longitude, altitude, out x, out y, out z);
            ecef_to_ned(x, y, z, ref_latitude, ref_longitude, ref_altitude, out north, out east, out down);
        }

        public static void ned_to_geodetic(double north, double east, double down,
                                            double ref_latitude, double ref_longitude, double ref_altitude,
                                             out double latitude, out double longitude, out double altitude)
        {
            // Local NED position to geodetic coordinates
            double x, y, z;
            ned_to_ecef(north, east, down, ref_latitude, ref_longitude, ref_altitude, out x, out y, out z);
            ecef_to_geodetic(x, y, z, out latitude, out longitude, out altitude);
        }

        public static void geodetic_to_enu(double latitude, double longitude, double altitude,
                             double ref_latitude, double ref_longitude, double ref_altitude,
                             out double east, out double north, out double up)
        {
            // Geodetic position to local ENU frame
            double x, y, z;
            geodetic_to_ecef(latitude, longitude, altitude, out x, out y, out z);

            double aux_north, aux_east, aux_down;
            ecef_to_ned(x, y, z, ref_latitude, ref_longitude, ref_altitude, out aux_north, out aux_east, out aux_down);

            east = aux_east;
            north = aux_north;
            up = -aux_down;
        }

        public static void enu_to_geodetic(double east, double north, double up,
                                           double ref_latitude, double ref_longitude, double ref_altitude,
                                           out double latitude, out double longitude, out double altitude)
        {
            // Local ENU position to geodetic coordinates

            double aux_north = north;
            double aux_east = east;
            double aux_down = -up;
            double x, y, z;
            ned_to_ecef(aux_north, aux_east, aux_down, ref_latitude, ref_longitude, ref_altitude, out x, out y, out z);
            ecef_to_geodetic(x, y, z, out latitude, out longitude, out altitude);
        }

        public static void geodetic_to_enu_simple(double latitude, double longitude, double altitude,
                                    double ref_latitude, double ref_longitude, double ref_altitude,
                                    out double east, out double north, out double up)
        {
            double dx = longitude - ref_longitude;
            double dy = latitude - ref_latitude;
            double dz = altitude - ref_altitude;
            east = deg2Rad(dx) * kSemimajorAxis * Math.Cos(deg2Rad(ref_latitude));
            north = deg2Rad(dy) * kSemimajorAxis;
            up = dz;
        }

        public static void enu_to_geodetic_simple(double east, double north, double up,
                                            double ref_latitude, double ref_longitude, double ref_altitude,
                                    out double latitude, out double longitude, out double altitude)
        {
            latitude = ref_latitude + rad2Deg(north / kSemimajorAxis);
            longitude = ref_longitude + rad2Deg(east / (kSemimajorAxis * Math.Cos(deg2Rad(ref_latitude))));
            altitude = ref_altitude + up;
        }

    }
}


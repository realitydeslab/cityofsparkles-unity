/////////////////////////////////////////////////////////////////////////////////////////////////////
//
// Audiokinetic Wwise generated include file. Do not edit.
//
/////////////////////////////////////////////////////////////////////////////////////////////////////

#ifndef __WWISE_IDS_H__
#define __WWISE_IDS_H__

#include <AK/SoundEngine/Common/AkTypes.h>

namespace AK
{
    namespace EVENTS
    {
        static const AkUniqueID PLAY_HEARTHEWINDWHISPER = 3656440737U;
        static const AkUniqueID PLAY_PARTICLECITY_IMPROV_BGM = 2470127684U;
        static const AkUniqueID PLAY_SENTIMENTBGMTEST = 2937301117U;
        static const AkUniqueID PLAY_STORYBGM = 973706497U;
        static const AkUniqueID PLAY_TWEET_LONG = 2998930726U;
        static const AkUniqueID PLAY_TWEET_MED = 2959931726U;
        static const AkUniqueID PLAY_TWEET_MED_LONG = 3560716643U;
        static const AkUniqueID PLAY_TWEET_SHORT = 241412554U;
        static const AkUniqueID PLAY_TWIST_BGM = 1993745496U;
        static const AkUniqueID PLAY_TWISTNOISYPART = 3980605832U;
        static const AkUniqueID PLAY_TWITTERBGM = 914952687U;
    } // namespace EVENTS

    namespace STATES
    {
        namespace RICHSENTIMENTTEST
        {
            static const AkUniqueID GROUP = 3873692266U;

            namespace STATE
            {
                static const AkUniqueID HAPPY = 1427264549U;
                static const AkUniqueID NEUTRAL = 670611050U;
                static const AkUniqueID SAD = 443572635U;
                static const AkUniqueID WISH = 1587393848U;
            } // namespace STATE
        } // namespace RICHSENTIMENTTEST

        namespace SENTIMENT
        {
            static const AkUniqueID GROUP = 610169148U;

            namespace STATE
            {
                static const AkUniqueID NEGATIVE = 4219547688U;
                static const AkUniqueID POSITIVE = 1192865152U;
            } // namespace STATE
        } // namespace SENTIMENT

        namespace STAGE
        {
            static const AkUniqueID GROUP = 1063701865U;

            namespace STATE
            {
                static const AkUniqueID FINALSPAWN = 1528378184U;
                static const AkUniqueID FIRST = 998496889U;
                static const AkUniqueID INITIALDARK = 3276465343U;
                static const AkUniqueID INTRO = 1125500713U;
                static const AkUniqueID LAST = 489968869U;
                static const AkUniqueID TWIST = 451915906U;
            } // namespace STATE
        } // namespace STAGE

    } // namespace STATES

    namespace GAME_PARAMETERS
    {
        static const AkUniqueID DENSITY = 1551159691U;
        static const AkUniqueID DISTANCETOPOI = 749878685U;
        static const AkUniqueID MASTERVOLUME = 2918011349U;
        static const AkUniqueID MIXINRATIO = 3473590661U;
        static const AkUniqueID SENTIMENTRATIO = 1059072165U;
    } // namespace GAME_PARAMETERS

    namespace BANKS
    {
        static const AkUniqueID INIT = 1355168291U;
        static const AkUniqueID TESTSOUNDBANK = 1831431028U;
    } // namespace BANKS

    namespace BUSSES
    {
        static const AkUniqueID _3D = 511093792U;
        static const AkUniqueID MASTER_AUDIO_BUS = 3803692087U;
        static const AkUniqueID MASTER_SECONDARY_BUS = 805203703U;
    } // namespace BUSSES

    namespace AUDIO_DEVICES
    {
        static const AkUniqueID NO_OUTPUT = 2317455096U;
        static const AkUniqueID SYSTEM = 3859886410U;
    } // namespace AUDIO_DEVICES

}// namespace AK

#endif // __WWISE_IDS_H__

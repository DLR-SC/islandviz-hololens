﻿using HoloIslandVis.Core;
using HoloToolkit.Sharing.SyncModel;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloIslandVis.Sharing
{
    [SyncDataClass]
    public class SyncDataStorage : SingletonComponent<SyncDataStorage>
    {
        public AppConfig AppConfig;

        [SyncData] public SyncBool SyncServerConnected;
        [SyncData] public SyncArray<SyncStorageFloat> SyncGenerator;

        public List<double> Generator { get; private set; }

        private int _usedGeneratorNumbers;

        void Start()
        {
            Generator = new List<double>()
            {
                0.5618161129,0.1458518864,0.3167425326,0.8483086502,0.8856986379,0.105661185,0.9705310822,0.1859217976,0.2267380358,0.1056256081,0.5579340577,0.2881707369,0.2636798188,0.9166777809,0.0590469316,0.3264687282,0.7710353955,0.1306145541,0.5304869905,0.4410409077,0.1796708057,0.7165998946,0.2717166367,0.8289072587,0.164534587,0.3493833897,0.1390452511,0.6494767715,0.8301713745,0.4858849856,0.1949898597,0.8812679205,0.2921479485,0.7487279888,0.2797045993,0.9731261269,0.2999575894,0.8150972867,0.1899165293,0.2603263656,0.9784876006,0.5487386913,0.5839925802,0.8050562929,0.0082488461,0.9871643847,0.3153061766,0.9542148765,0.8745195655,0.4529218937,0.1105445126,0.0664556604,0.8791958745,0.8736377609,0.1016154294,0.6639775106,0.413957847,0.7808996941,0.0225489359,0.2083302667,0.1512068748,0.8689858876,0.6175554453,0.4095958357,0.93482255,0.4297945301,0.6312674962,0.3810817568,0.4432073992,0.5232195042,0.5788240892,0.7554564112,0.6544407688,0.7598542845,0.8780222795,0.1973439209,0.7625680984,0.7947934013,0.5918092446,0.351298548,0.3962301018,0.4771405707,0.9432434817,0.3206109061,0.6059142964,0.1522190651,0.8043065643,0.4302489601,0.5769639223,0.193736665,0.1528601195,0.4328980886,0.4429459448,0.2623520015,0.3163111258,0.8795443684,0.2259764253,0.4156882467,0.909925716,0.360917587f,
                0.6407799756,0.9036725997,0.9617502256,0.2910444156,0.828824372,0.2722943659,0.1498757774,0.4484669,0.6619747591,0.1849893863,0.1554468107,0.7497006407,0.6202234652,0.781738934,0.9743099362,0.4177236317,0.7857496747,0.1499977001,0.2296640217,0.3394967412,0.7219541798,0.105867432,0.2113625655,0.1648082929,0.2189156638,0.4838536975,0.633893085,0.4967227296,0.0492613446,0.0359311523,0.5262105598,0.7668527135,0.1968214825,0.3560287842,0.9184266347,0.9931550291,0.7671940056,0.3483115338,0.5378487886,0.6146759249,0.5804068803,0.7760343308,0.731987507,0.8515332187,0.7969771544,0.2366709012,0.0574156717,0.0877476056,0.7843804042,0.4584936488,0.2295016647,0.3542366919,0.0193956527,0.9968319568,0.1861934276,0.807153426,0.8696767403,0.9900521273,0.8295650491,0.3081888511,0.7123990668,0.14583983,0.4226566057,0.0090685152,0.3793786403,0.5134420602,0.6352180045,0.4506614834,0.3614009057,0.6691833286,0.5003393393,0.7420561736,0.7780110313,0.7247447459,0.6061450078,0.1960050567,0.0666835774,0.2256018441,0.9344824371,0.2409089172,0.535924678,0.579868208,0.371304251,0.0170836169,0.7347071118,0.4409712518,0.0088978838,0.7426653894,0.1940150797,0.7676435456,0.3804223558,0.6097953579,0.8104396937,0.5949731896,0.9689943134,0.9170721653,0.5617412918,0.3788407442,0.0598834441,0.6764396358,
                0.3858654696,0.0911620316,0.8150336295,0.7004833565,0.1835280644,0.9277554592,0.4734624012,0.0461847419,0.3763499681,0.9213084275,0.2043168113,0.5693622923,0.3919070966,0.8472688048,0.1278475207,0.6468599158,0.3531960204,0.4506409594,0.1136567882,0.5758999505,0.5900166168,0.2433087035,0.3511312829,0.0266924091,0.993587872,0.8909532297,0.3597861479,0.9602998532,0.4151418383,0.8747489005,0.4477460568,0.0841822671,0.1516892128,0.615120028,0.4681287974,0.9471624871,0.4513806838,0.6173228224,0.3454331739,0.2406612142,0.0420720894,0.263072943,0.4261712997,0.0377150322,0.9131249287,0.8127086707,0.7532474952,0.5715815535,0.427687274,0.7222951328,0.5279628548,0.0664492646,0.7078562471,0.0780522949,0.0319786668,0.5533907286,0.4641381807,0.8214252013,0.2650591579,0.5269995241,0.589359212,0.7268156138,0.4997521418,0.0873163124,0.5802056992,0.2041431746,0.8689036238,0.2951158757,0.8489938764,0.9446486281,0.019614839,0.3181843922,0.5635893259,0.4987532764,0.2719609487,0.5645088617,0.112205653,0.2813482603,0.364185889,0.4770293289,0.2816564847,0.2188759112,0.0471618111,0.3270721987,0.3827439967,0.0738513703,0.7053385888,0.3291152219,0.8044108114,0.309586757,0.3887277038,0.8181131966,0.9298378508,0.8996807523,0.314497912,0.6008231354,0.2684456814,0.2652711446,0.1683693301,0.5288721651
            };

            _usedGeneratorNumbers = 0;
        }

        public float GetRandomNumber(System.Random random)
        {
            float value = (float)Generator[_usedGeneratorNumbers % 299];
            _usedGeneratorNumbers++;
            return value;

            //if (!AppConfig.SharingEnabled)
            //    return (float)random.NextDouble();
            //
            //if(AppConfig.IsServerInstance)
            //{
            //    float value = (float)random.NextDouble();
            //    SyncStorageFloat syncValue = new SyncStorageFloat();
            //
            //    syncValue.Value = value;
            //    SyncGenerator.AddObject(syncValue);
            //    Generator.Add(value);
            //    return value;
            //}
            //else
            //{
            //    float value = (float)Generator[_usedGeneratorNumbers];
            //    _usedGeneratorNumbers++;
            //    return value;
            //}
        }
    }
}
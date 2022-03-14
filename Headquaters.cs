using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ATNC;

public static class Headquaters {
	private static readonly Dictionary<byte, string> _errors = new() {
		{ 0, "The tire has been punctured, stop and replace the flat tire with a spare." },                 // Píchlá pneumatika
		{ 1, "You're out of gas, push the car to the side of the road and refuel." },                                   // Došel benzín
		{ 2, "Car engine error (clutch violation) - start the engine." },                                                           // Motor - spojka
		{ 3, "Car engine error - look for the nearest car service." },                                                          // Motor - poničení
		{ 4, "Unknown error - look for the nearest car service." },                                                             // Neznámá chyba
	};
	public static readonly Dictionary<string, Func<string, string>> commands = new() {
		{
			"ggps",
			(s) => @"t0;4595;200;_ b4595;7959;200;_ b12554;8643;200;_ t21197;8988;200;_ n30185;5173;200;_ b35358;704;200;_ b36062;7749;200;_ t43811;811;200;_ b44622;8111;200;_ b52733;6894;200;_ b59627;8197;200;_ t67824;4325;200;_ n72149;7003;200;_ b79152;721;200;_ t79873;1379;200;Brno t81252;5811;200;_ b87063;942;200;_ b88005;9245;200;_ b97250;8002;200;_ b105252;4145;200;_ n109397;1971;200;_ n111368;3156;200;_ t114524;4021;200;_ n118545;7804;200;_ t126349;8746;200;_ t135095;5897;200;_ b140992;4561;200;_ t145553;6766;200;_ n152319;7750;200;_ b160069;3723;200;_ b163792;5478;200;Plzen t169270;4666;200;_ b173936;1184;200;_ t175120;9497;200;_ b184617;1108;200;_ b185725;1712;200;_ b187437;8384;200;_ b195821;2743;200;_ n198564;5876;200;_ n204440;1867;200;_ n206307;5438;200;_ n211745;4588;200;_ n216333;5460;200;_ b221793;1389;200;Usti_nad_Orlici n223182;8198;200;_ n231380;4313;200;_ n235693;6579;200;_ t242272;1791;200;_ t244063;2868;200;_ n246931;3383;200;_ n250314;5290;200;_ n255604;2110;200;_ b257714;6863;200;_ b264577;7551;200;_ t272128;7566;200;_ t279694;1071;200;Zlin b280765;1748;200;_ t282513;5053;200;_ n287566;965;200;_ n288531;1315;200;_ t289846;2097;200;_ n291943;6731;200;_ b298674;4290;200;_ n302964;7827;200;_ t310791;2631;200;_ b313422;2279;200;Praha t315701;1444;200;_ b317145;5844;200;_ t322989;855;200;_ b323844;6218;200;_ t330062;2370;200;_ t332432;2063;200;Usti_nad_Labem n334495;8776;200;_ n343271;8541;200;_ b351812;5207;200;_ n357019;9700;200;_ t366719;4355;200;_ b371074;4166;200;_ t375240;9062;200;_ t384302;7811;200;Jindrichuv_Hradec b392113;7027;200;_ n399140;8383;200;_ t407523;5657;200;_ b413180;5189;200;_ t418369;3988;200;_ n422357;2753;200;_ t425110;4685;200;_ b429795;2736;200;_ n432531;1097;200;_ b433628;7280;200;_ b440908;1873;200;_ b442781;3470;200;_ b446251;3612;200;_ t449863;3721;200;Ostrava b453584;7222;200;_ b460806;9681;200;_ t470487;3209;200;_ t473696;6067;200;_ t479763;5329;200;_ b485092;7312;200;_ b492404;6234;200;_ t498638;7651;200;_ n506289;5966;200;_ t512255;6486;200;_ t518741;6456;200;_ b525197;7878;200;_ t533075;2765;200;_ n535840;5176;200;_ t541016;778;200;Pardubice n541794;738;200;_ n542532;5037;200;_ b547569;1802;200;_ b549371;8383;200;_ n557754;547;200;_ n558301;4517;200;Prelouc n562818;2782;200;_ t565600;2891;200;_ b568491;9758;200;_ b578249;7850;200;_ n586099;9414;200;_ n595513;9366;200;_ b604879;9927;200;_ n614806;4132;200;_ t618938;1265;200;_ b620203;5285;200;_ t625488;5370;200;_ t630858;3271;200;_ n634129;3213;200;Hradec_Kralove t637342;5591;200;_ b642933;7309;200;_ b650242;870;200;_ n651112;6862;200;_ n657974;9444;200;_ t667418;4027;200;_ b671445;5248;200;_ t676693;4407;200;_ b681100;5385;200;_ n686485;7087;200;_ t693572;1149;200;_ t694721;728;200;_ n695449;6267;200;Karvina n701716;4674;200;_ t706390;2854;200;_ t709244;3173;200;_ b712417;4956;200;_ t717373;7522;200;_ n724895;3261;200;_ n728156;9136;200;_ n737292;2043;200;_ b739335;5698;200;_ n745033;5023;200;_ b750056;9941;200;_ n759997;7221;200;_ t767218;4778;200;_ b771996;4418;200;_ n776414;4975;200;Kladno"
		},
		{
			"err",
			(s) => {
				string str;

				try {
					str = _errors[byte.Parse(s)];
				} catch (FormatException) {
					str = "-";
				} catch (KeyNotFoundException) {
					str = "-";
				} catch (OverflowException) {
					str = "-";
				}

				return str;
			}
		}
	};

	internal static class MeteoStation {
		public delegate void WeatherChangedDelegate(WeatherChangedEventArgs weatherChangedEventArgs);
		public static event WeatherChangedDelegate WeatherChanged;
		private static readonly Random _rnd = new();

		private const sbyte _min = -20, _max = 30;
		private static volatile sbyte _wet;

		public static sbyte Weather {
			get => _wet;
			private set {
				_wet = value;
				WeatherChanged?.Invoke(new() { Weather = _wet });
			}
		}

		static MeteoStation() => StartTimer();

		private static async void StartTimer() {
			Weather = (sbyte)_rnd.Next(_min, _max);
			await Task.Delay(_rnd.Next(10000, 120000));
			StartTimer();
		}

		public class WeatherChangedEventArgs : EventArgs {
			public sbyte Weather { init; get; }
		}
	}
}

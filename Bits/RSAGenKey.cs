using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace Bits {
	static class RSAGenKey {
		static private uint[] primeList = {
			5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47, 53, 59, 61, 67, 71, 73, 79,
			83, 89, 97, 101, 103, 107, 109, 113, 127, 131, 133, 137, 139, 149, 151, 157,
			163, 167, 173, 179, 181, 191, 193, 197, 199, 211, 223, 227, 229, 233, 239,
			241, 251, 257, 263, 269, 271, 277, 281, 283, 293, 307, 311, 313, 317, 331,
			337, 347, 349, 353, 359, 367, 373, 379, 383, 389, 397, 401, 409, 419, 421,
			425, 431, 433, 439, 443, 449, 457, 461, 463, 467, 479, 487, 491, 499, 503,
			509, 521, 523, 541, 547, 557, 563, 569, 571, 577, 587, 593, 599, 601, 607,
			613, 617, 619, 631, 641, 643, 647, 653, 659, 661, 673, 677, 683, 691, 701,
			709, 719, 727, 733, 739, 743, 751, 757, 761, 769, 773, 787, 797, 809, 811,
			821, 823, 827, 829, 839, 853, 857, 859, 863, 877, 881, 883, 887, 907, 911,
			919, 929, 937, 941, 947, 953, 967, 971, 977, 983, 991, 997, 1009, 1013, 1019,
			1021, 1031, 1033, 1039, 1049, 1051, 1061, 1063, 1069, 1087, 1091, 1093, 1097,
			1103, 1109, 1117, 1123, 1129, 1151, 1153, 1163, 1171, 1181, 1187, 1193, 1201,
			1213, 1217, 1223, 1229, 1231, 1237, 1249, 1259, 1277, 1279, 1283, 1289, 1291,
			1297, 1301, 1303, 1307, 1319, 1321, 1327, 1361, 1367, 1373, 1381, 1399, 1409,
			1423, 1427, 1429, 1433, 1439, 1447, 1451, 1453, 1459, 1471, 1481, 1483, 1487,
			1489, 1493, 1499, 1511, 1523, 1531, 1541, 1543, 1549, 1553, 1559, 1567, 1571,
			1579, 1583, 1597, 1601, 1607, 1609, 1613, 1619, 1621, 1627, 1637, 1657, 1663,
			1667, 1669, 1693, 1697, 1699, 1709, 1721, 1723, 1733, 1741, 1747, 1753, 1759,
			1777, 1783, 1787, 1789, 1801, 1811, 1823, 1831, 1847, 1861, 1867, 1871, 1873,
			1877, 1879, 1889, 1891, 1901, 1907, 1913, 1931, 1933, 1949, 1951, 1973, 1979,
			1987, 1991, 1993, 1997, 1999, 2003, 2011, 2017, 2027, 2029, 2039, 2053, 2063,
			2069, 2081, 2083, 2087, 2089, 2099, 2101, 2111, 2113, 2129, 2131, 2137, 2141,
			2143, 2153, 2161, 2179, 2203, 2207, 2213, 2221, 2237, 2239, 2243, 2251, 2267,
			2269, 2273, 2281, 2287, 2293, 2297, 2309, 2311, 2321, 2333, 2339, 2341, 2347,
			2351, 2357, 2371, 2377, 2381, 2383, 2389, 2393, 2399, 2411, 2413, 2417, 2423,
			2437, 2441, 2447, 2459, 2467, 2473, 2477, 2503, 2521, 2531, 2539, 2543, 2549,
			2551, 2557, 2579, 2584, 2591, 2593, 2609, 2617, 2621, 2633, 2647, 2657, 2659,
			2663, 2671, 2677, 2683, 2687, 2689, 2693, 2699,
		};

        static public BigInteger GCD(BigInteger a, BigInteger b) {
			BigInteger tmp;
			while(b != 0) {
				tmp = b;
				b = a % b;
				a = tmp;
			}
			return a;
		}

		static private BigInteger GDCExtended(BigInteger a, BigInteger b, ref BigInteger x, ref BigInteger y) {
			if(a == 0) {
				x = 0;
				y = 1;
				return b;
			}
			BigInteger x1 = 0;
			BigInteger y1 = 0;
			BigInteger gcd = GDCExtended(b % a, a, ref x1, ref y1);
			x = y1 - (b / a) * x1;
			y = x1;
			return gcd;
		}

		static int BitCount(BigInteger number) {
			if(number == 0)
				return 0;
			int count = 0;
            BigInteger temp = 0;
			for(int i = 10; i >= 0; i--) {
				temp = number >> (1 << i);
				if(temp != 0) {
					number = temp;
					count += (1 << i);
				}
			}
			return count + 1;
		}

		static BigInteger ModPower(ulong a, BigInteger b, BigInteger p) {
			int N = BitCount(b);
			BigInteger x = a % p;
			BigInteger temp;
			for(int i = N - 2; i >= 0; i--) {
				temp = b >> i;
				if((temp & 0x01) == 0)
					x = (x * x) % p;
				else
					x = (((x * x) % p) * a) % p;
			}

			return x;
		}

		static void Decompose(BigInteger p, ref int k, ref BigInteger m) {
			int i = 0;
			while((p & 0x01) == 0) {
				i++;
				p = p >> 1;
			}
			k = i;
			m = p;
		}

		static private bool Witness(ulong a, BigInteger number) {
			int k = 0;
			BigInteger m = new BigInteger();
			Decompose(number - 1, ref k, ref m);
			BigInteger[] B = new BigInteger[k + 1];
			B[0] = ModPower(a, m, number);
			if(B[0] == 1)
				return true;
			else {
				int i = 1;
				while(i <= k) {
					B[i] = (B[i - 1] * B[i - 1]) % number;
					if(B[i] == 1) {
						if(B[i - 1] == number - 1)
							return true;
						else
							return false;
					}
					i++;
				}
			}
			return false;
		}

		static bool FermatCheck(BigInteger checkValue, ulong randomNumer) {
			if(ModPower(randomNumer, checkValue - 1, checkValue) != 1)
				return false;
			else
				return true;
		}

		static private bool MillerRabinCheck(BigInteger checkValue, ulong randomNumer) {
			return Witness(randomNumer, checkValue);
        }

		static private bool PrimeCheck(BigInteger checkValue) {
			for(int i = 0; i < primeList.Length; i++) {
				if((checkValue % primeList[i]) == 0)
					return false;
            }
			Random rnd = new Random();
			ulong randomNumer = (ulong)(2 + rnd.NextInt64() % (checkValue - 3));
			if(GCD(randomNumer, checkValue) != 1)
				return false;
			else if(!FermatCheck(checkValue, randomNumer))
				return false;
			else
				return MillerRabinCheck(checkValue, randomNumer);
		}

		static public BigInteger GenPrime(int bitSize) {
			int N = bitSize / 64;
			if(bitSize % 64 != 0)
				N++;
			Random rnd = new Random();
			BigInteger temp = 0;
			BigInteger maxValue = ((new BigInteger(1)) << bitSize) - 1;
			for(int i = 0; i < N; i++) {
				temp <<= 64;
				temp |= (ulong)rnd.NextInt64();
			}
			temp &= (~(maxValue << bitSize)) & maxValue;
			temp |= (new BigInteger(1)) << (bitSize - 1);
			temp = temp / 6 * 6;
			while(temp <= maxValue) {
				if(PrimeCheck(temp - 1))
					return temp - 1;
				else if(PrimeCheck(temp + 1))
					return temp + 1;
				temp += 6;
			}
			return 0;
        }

		static public BigInteger GenPrivateExponent(BigInteger phi, uint publicExponent) {
			BigInteger x = 0, y = 0;
			GDCExtended(phi, publicExponent, ref x, ref y);
			if(y < 0)
				y += phi;
			return y;
		}
    }
}

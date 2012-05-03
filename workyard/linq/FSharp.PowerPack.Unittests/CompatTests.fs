namespace FSharp.PowerPack.Unittests
open NUnit.Framework
open System.Collections.Generic

exception Foo

#nowarn "44"

[<TestFixture>]
type public ArrayTests() =
  
    [<Test>]
    member this.BasicTests() = 
        let test_mem () = 
          test "array.contains a" (not (Array.contains 3 [| 1; 2; 4 |]))
          test "array.contains b" (Array.contains 3 [| 1; 3; 4 |])

        let test_make_matrix () = 
          let arr = Array.createJaggedMatrix 2 3 6 in
          test "test2931: sdvjk2" (arr.[0].[0] = 6);
          test "test2931: sdvjk2" (arr.[0].[1] = 6);
          test "test2931: sdvjk2" (arr.[0].[2] = 6);
          test "test2931: sdvjk2" (arr.[1].[0] = 6);
          test "test2931: sdvjk2" (arr.[1].[1] = 6);
          test "test2931: sdvjk2" (arr.[1].[2] = 6);
          arr.[0].[0] <- 5;
          arr.[0].[1] <- 5;
          arr.[0].[2] <- 5;
          arr.[1].[0] <- 4;
          arr.[1].[1] <- 5;
          arr.[1].[2] <- 5;
          test "test2931: sdvjk2" (arr.[1].[0] = 4)

        test_make_matrix ()
        test_mem ()

[<TestFixture>]
type public ``Byte``() =
  
    [<Test>]
    member this.BasicTests() = 

      test "vwknjewv0" (Byte.zero = 0uy);
      test "vwknjewv0"  (Byte.add 0uy Byte.one = Byte.one);
      test  "vwknjewv0" (Byte.add Byte.one 0uy  = Byte.one);
      test  "vwknjewv0" (Byte.sub Byte.one 0uy  = Byte.one);
      test  "vwknjewv0" (Byte.sub Byte.one Byte.one  = 0uy);
      for i = 0 to 255 do 
        test  "vwknjewv0" (int (byte i) = i);
      for i = 0 to 255 do 
        test  "vwknjewv0" (byte i = byte i);
      stdout.WriteLine "mul i 1";
      for i = 0 to 255 do 
        test  "vwknjewv0"  (Byte.mul (byte i) (byte 1) = byte i);
      stdout.WriteLine "add";
      for i = 0 to 255 do 
        for j = 0 to 255 do 
          test  "vwknjewv0"  (int (Byte.add (byte i) (byte j)) = ((i + j) % 256));
      stdout.WriteLine "mul i 1";
      for i = 0 to 49032 do 
        test  "vwknjewv0"  (int (byte i) = (i % 256));
      for i = 0 to 255 do 
        test  "vwknjewv0"  (Byte.div (byte i) (byte 1) = byte i);
      for i = 0 to 255 do 
        test  "vwknjewv0"  (Byte.rem (byte i) (byte 1) = byte 0);
      for i = 0 to 254 do 
        test  "vwknjewv0"  (Byte.succ (byte i) = Byte.add (byte i) Byte.one);
      for i = 1 to 255 do 
        test  "vwknjewv0"  (Byte.pred (byte i) = Byte.sub (byte i) Byte.one);
      for i = 1 to 255 do 
        for j = 1 to 255 do 
          test  "vwknjewv0"  (int (Byte.logand (byte i) (byte j)) = (&&&) i j);
      stdout.WriteLine "logor";
      for i = 1 to 255 do 
        for j = 1 to 255 do 
          test  "vwknjewv0"  (int (Byte.logor (byte i) (byte j)) = (|||) i j);
      stdout.WriteLine "logxor";
      for i = 1 to 255 do 
        for j = 1 to 255 do 
          test  "vwknjewv0"  (int (Byte.logxor (byte i) (byte j)) = (^^^) i j);
      stdout.WriteLine "lognot";
      for i = 1 to 255 do 
          test  "vwknjewv0"  (Byte.lognot (byte i) = byte (~~~ i))
      stdout.WriteLine "shift_left";
      for i = 0 to 255 do 
        for j = 0 to 7 do 
          test  "vwknjewv0"  (Byte.shift_left (byte i) j = byte ( i <<< j))
      stdout.WriteLine "shift_right";
      for i = 0 to 255 do 
        for j = 0 to 7 do 
          test  "vwknjewv0"  (Byte.shift_right (byte i) j = byte (i >>> j))
      stdout.WriteLine "to_string";
      for i = 0 to 255 do 
          test  "vwknjewv0"  (Byte.to_string (byte i) = sprintf "%d" i)
      stdout.WriteLine "of_string";
      for i = 0 to 255 do 
          test  "vwknjewv0"  (Byte.of_string (string i) = byte i)
      stdout.WriteLine "done";
      ()    


[<TestFixture>]
type public Int32Tests() =
  
    [<Test>]
    member this.BasicTests() = 
      test "test1" (Int32.zero = Int32.zero);
      test "test2" (Int32.add Int32.zero Int32.one = Int32.one);
      test "test3" (Int32.add Int32.one Int32.zero  = Int32.one);
      test "test4" (Int32.sub Int32.one Int32.zero  = Int32.one);
      test "test5" (Int32.sub Int32.one Int32.one  = Int32.zero);
      for i = 0 to 255 do 
        test "test6" (Int32.to_int (Int32.of_int i) = i);
      done;
      for i = 0 to 255 do 
        test "test7" (Int32.of_int i = Int32.of_int i);
      done;
      stdout.WriteLine "mul i 1";
      for i = 0 to 255 do 
        test "test8" (Int32.mul (Int32.of_int i) (Int32.of_int 1) = Int32.of_int i);
      done;
      stdout.WriteLine "add";
      for i = 0 to 255 do 
        for j = 0 to 255 do 
          test "test" (Int32.to_int (Int32.add (Int32.of_int i) (Int32.of_int j)) = (i + j));
        done;
      done;
      stdout.WriteLine "constants: min_int"; stdout.Flush();
      test "testq" (Int32.min_int = -2147483648);
      test "testw" (Int32.min_int = -2147483647 - 1);

      stdout.WriteLine "constants: max_int";stdout.Flush();
      test "teste" (Int32.max_int = 2147483647);
      test "testr" (Int32.max_int = 2147483646 + 1);

      stdout.WriteLine "constants: string max_int";stdout.Flush();
      test "testt" (string Int32.max_int = "2147483647");
      test "testy" (string Int32.min_int = "-2147483648");
      test "testu" (Int32.to_string Int32.max_int = "2147483647");
      test "testi" (Int32.to_string Int32.min_int = "-2147483648");

      stdout.WriteLine "constants: max_int - 10";stdout.Flush();
      test "testa" (Int32.max_int - 10 = 2147483637);

      stdout.WriteLine "min int";stdout.Flush();
      for i = Int32.min_int to Int32.min_int + 10 do 
        test "testb" (Int32.to_int (Int32.of_int i) = i);
      done;
      stdout.WriteLine "max int";stdout.Flush();
      for i = Int32.max_int - 10 to Int32.max_int - 1 do 
        test "testc" (Int32.to_int (Int32.of_int i) = i);
      done;
      stdout.WriteLine "div";
      for i = 0 to 255 do 
        test "testd" (Int32.div (Int32.of_int i) (Int32.of_int 1) = Int32.of_int i);
      done;
      for i = 0 to 255 do 
        test "teste" (Int32.rem (Int32.of_int i) (Int32.of_int 1) = Int32.of_int 0);
      done;
      for i = 0 to 254 do 
        test "testf" (Int32.succ (Int32.of_int i) = Int32.add (Int32.of_int i) Int32.one);
      done;
      for i = 1 to 255 do 
        test "testg" (Int32.pred (Int32.of_int i) = Int32.sub (Int32.of_int i) Int32.one);
      done;
      for i = 1 to 255 do 
        for j = 1 to 255 do 
          test "testh" (Int32.to_int (Int32.logand (Int32.of_int i) (Int32.of_int j)) = (i &&& j));
        done;
      done;
      stdout.WriteLine "logor";
      for i = 1 to 255 do 
        for j = 1 to 255 do 
          test "testj" (Int32.to_int (Int32.logor (Int32.of_int i) (Int32.of_int j)) = (i ||| j));
        done;
      done;
      stdout.WriteLine "logxor";
      for i = 1 to 255 do 
        for j = 1 to 255 do 
          test "testkkh" (Int32.to_int (Int32.logxor (Int32.of_int i) (Int32.of_int j)) = (i ^^^ j));
        done;
      done;
      stdout.WriteLine "lognot";
      for i = 1 to 255 do 
          test "testqf" (Int32.lognot (Int32.of_int i) = Int32.of_int (~~~i))
      done;
      stdout.WriteLine "shift_left";
      for i = 0 to 255 do 
        for j = 0 to 7 do 
          test "testcr4" (Int32.shift_left (Int32.of_int i) j = Int32.of_int (i <<< j))
        done;
      done;
      stdout.WriteLine "shift_right";
      for i = 0 to 255 do 
        for j = 0 to 7 do 
          test "testvt3q" (Int32.shift_right (Int32.of_int i) j = Int32.of_int (i >>> j))
        done;
      done;
      test "testqvt4" (Int32.shift_right 2 1 = 1);
      test "testvq3t" (Int32.shift_right 4 2 = 1);
      stdout.WriteLine "shift_right_logical";
      for i = 0 to 255 do 
        for j = 0 to 7 do 
          test "testvq34" (Int32.shift_right_logical (Int32.of_int i) j = Int32.of_int (Pervasives.(lsr) i j))
        done;
      done;
      test "testvq3t" (Int32.shift_right_logical 0xFFFFFFFF 1 = 0x7FFFFFFF);
      stdout.WriteLine "shift_right_logical (1)";
      test "testvq3" (Int32.shift_right_logical 0xFFFFFFF2 1 = 0x7FFFFFF9);
      stdout.WriteLine "shift_right_logical (2)";
      test "testqvt4" (Int32.shift_right_logical 0x7FFFFFF2 1 = 0x3FFFFFF9);
      stdout.WriteLine "shift_right_logical (3) ";
      test "testqv3t" (Int32.shift_right_logical 0xFFFFFFFF 2 = 0x3FFFFFFF);
      stdout.WriteLine "shift_right_logical (4)";
      test "testb4y5" (Int32.shift_right_logical 0x80000004 2 = 0x20000001);
      stdout.WriteLine "to_string";
      for i = 0 to 255 do 
          test "testbsyet" (Int32.to_string (Int32.of_int i) = string i)
      done;
      stdout.WriteLine "of_string";
      for i = 0 to 255 do 
          test "testvq4" (Int32.of_string (string i) = Int32.of_int i)
      done;
      stdout.WriteLine "constants (hex)";
      test "testv4w" (Int32.of_string "0x0" = 0);
      test "testv35" (Int32.of_string "0x1" = 1);
      test "testvq3" (Int32.of_string "0x2" = 2);
      test "testv3qt" (Int32.of_string "0xa" = 10);
      test "testbwy4" (Int32.of_string "0xff" = 255);
      stdout.WriteLine "constants (octal)";
      test "testb4y5" (Int32.of_string "0o0" = 0);
      test "testb4y" (Int32.of_string "0o1" = 1);
      test "testby4" (Int32.of_string "0o2" = 2);
      test "testbw4y" (Int32.of_string "0o7" = 7);
      test "testb45" (Int32.of_string "0o10" = 8);
      test "testbw4" (Int32.of_string "0o777" = 7*64 + 7*8 + 7);
      test "test67n" (Int32.of_string "0o111" = 64 + 8 + 1);
      stdout.WriteLine "constants (binary)";
      test "test34q" (Int32.of_string "0b0" = 0);
      test "testn" (Int32.of_string "0b1" = 1);
      test "tester" (Int32.of_string "0b10" = 2);
      test "testeyn" (Int32.of_string "0b11" = 3);
      test "testynr" (Int32.of_string "0b00000000" = 0);
      test "testnea" (Int32.of_string "0b11111111" = 0xFF);
      test "testneayr" (Int32.of_string "0b1111111100000000" = 0xFF00);
      test "testne" (Int32.of_string "0b11111111000000001111111100000000" = 0xFF00FF00);
      test "testnaey" (Int32.of_string "0b11111111111111111111111111111111" = 0xFFFFFFFF);
      test "testny" (Int32.of_string "0x7fffffff" = Int32.max_int);

[<TestFixture>]
type public UInt32Tests() =
  
    [<Test>]
    member this.BasicTests() = 
      stdout.WriteLine "constants (hex, unit32)";
      test "testv4w" (UInt32.of_string "0x0" = 0u);
      test "testv35" (UInt32.of_string "0x1" = 1u);
      test "testvq3" (UInt32.of_string "0x2" = 2u);
      test "testv3qt" (UInt32.of_string "0xa" = 10u);
      test "testbwy4" (UInt32.of_string "0xff" = 255u);
      stdout.WriteLine "constants (octal, unit32)";
      test "testb4y5" (UInt32.of_string "0o0" = 0u);
      test "testb4y" (UInt32.of_string "0o1" = 1u);
      test "testby4" (UInt32.of_string "0o2" = 2u);
      test "testbw4y" (UInt32.of_string "0o7" = 7u);
      test "testb45" (UInt32.of_string "0o10" = 8u);
      test "testbw4" (UInt32.of_string "0o777" = 7u*64u + 7u*8u + 7u);
      test "test67n" (UInt32.of_string "0o111" = 64u + 8u + 1u);
      stdout.WriteLine "constants (binary, unit32)";

      test "test34q" (UInt32.of_string "0b0" = 0u);
      test "testn" (UInt32.of_string "0b1" = 1u);
      test "tester" (UInt32.of_string "0b10" = 2u);
      test "testeyn" (UInt32.of_string "0b11" = 3u);
      test "testynr" (UInt32.of_string "0b00000000" = 0u);
      test "testnea" (UInt32.of_string "0b11111111" = 0xFFu);
      test "testneayr" (UInt32.of_string "0b1111111100000000" = 0xFF00u);

      test "testne" (UInt32.of_string "0b11111111000000001111111100000000" = 0xFF00FF00u);
      test "testnaey" (UInt32.of_string "0b11111111111111111111111111111111" = 0xFFFFFFFFu);
      test "testny" (UInt32.of_string "0xffffffff" = UInt32.max_int);

      stdout.WriteLine "constants (decimal)";
      test "test" (Int32.of_string "2147483647" = Int32.max_int);
      test "test" (Int32.of_string "-0x80000000" = Int32.min_int);
      test "test" (Int32.of_string "-2147483648" = Int32.min_int);
      stdout.WriteLine "done";
      ()    

[<TestFixture>]
type public Int64Tests() =
  
    [<Test>]
    member this.BasicTests() = 
          test  "vwknw4vkl"  (Int64.zero = Int64.zero);
          test  "vwknw4vkl"  (Int64.add Int64.zero Int64.one = Int64.one);
          test  "vwknw4vkl"  (Int64.add Int64.one Int64.zero  = Int64.one);
          test  "vwknw4vkl"  (Int64.sub Int64.one Int64.zero  = Int64.one);
          test  "vwknw4vkl"  (Int64.sub Int64.one Int64.one  = Int64.zero);
          for i = 0 to 255 do 
            test  "vwknw4vkl"  (Int64.to_int (Int64.of_int i) = i);
          done;
          for i = 0 to 255 do 
            test  "vwknw4vkl"  (Int64.of_int i = Int64.of_int i);
          done;
          stdout.WriteLine "mul i 1";
          for i = 0 to 255 do 
            test  "vwknw4vkl"  (Int64.mul (Int64.of_int i) (Int64.of_int 1) = Int64.of_int i);
          done;
          stdout.WriteLine "add";
          for i = 0 to 255 do 
            for j = 0 to 255 do 
              test  "vwknw4vkl"  (Int64.to_int (Int64.add (Int64.of_int i) (Int64.of_int j)) = (i + j));
            done;
          done;
          stdout.WriteLine "div";
          for i = 0 to 255 do 
            test  "vwknw4vkl"  (Int64.div (Int64.of_int i) (Int64.of_int 1) = Int64.of_int i);
          done;
          for i = 0 to 255 do 
            test  "vwknw4vkl"  (Int64.rem (Int64.of_int i) (Int64.of_int 1) = Int64.of_int 0);
          done;
          for i = 0 to 254 do 
            test  "vwknw4vkl"  (Int64.succ (Int64.of_int i) = Int64.add (Int64.of_int i) Int64.one);
          done;
          for i = 1 to 255 do 
            test  "vwknw4vkl"  (Int64.pred (Int64.of_int i) = Int64.sub (Int64.of_int i) Int64.one);
          done;
          for i = 1 to 255 do 
            for j = 1 to 255 do 
              test  "vwknw4vkl"  (Int64.to_int (Int64.logand (Int64.of_int i) (Int64.of_int j)) = Pervasives.(land) i j);
            done;
          done;
          stdout.WriteLine "logor";
          for i = 1 to 255 do 
            for j = 1 to 255 do 
              test  "vwknw4vkl"  (Int64.to_int (Int64.logor (Int64.of_int i) (Int64.of_int j)) = Pervasives.(lor) i j);
            done;
          done;
          stdout.WriteLine "logxor";
          for i = 1 to 255 do 
            for j = 1 to 255 do 
              test  "vwknw4vkl"  (Int64.to_int (Int64.logxor (Int64.of_int i) (Int64.of_int j)) = i ^^^ j);
            done;
          done;
          stdout.WriteLine "lognot";
          for i = 1 to 255 do 
              test  "vwknw4vkl"  (Int64.lognot (Int64.of_int i) = Int64.of_int (~~~ i))
          done;
        #if NOTAILCALLS // NOTAILCALLS <-> MONO
        #else
          stdout.WriteLine "shift_left";
          for i = 0 to 255 do 
            for j = 0 to 7 do 
              test  "vwknw4vkl"  (Int64.shift_left (Int64.of_int i) j = Int64.of_int (i <<< j))
            done;
          done;
          stdout.WriteLine "shift_right";
          for i = 0 to 255 do 
            for j = 0 to 7 do 
              test  "vwknw4vkl"  (Int64.shift_right (Int64.of_int i) j = Int64.of_int (i >>> j))
            done;
          done;
          stdout.WriteLine "shift_right_logical";
          for i = 0 to 255 do 
            for j = 0 to 7 do 
              test  "vwknw4vkl"  (Int64.shift_right_logical (Int64.of_int i) j = Int64.of_int (Pervasives.(lsr) i j))
            done;
          done;
        #endif
          stdout.WriteLine "to_string";
          for i = 0 to 255 do 
              test  "vwknw4vkl"  (Int64.to_string (Int64.of_int i) = string i)
          done;
          stdout.WriteLine "of_string";
          for i = 0 to 255 do 
              test  "vwknw4vkl"  (Int64.of_string (string i) = Int64.of_int i)
          done;
          stdout.WriteLine "constants (hex)";
          test  "vwknw4vkl"  (Int64.of_string "0x0" = 0L);
          test  "vwknw4vkl"  (Int64.of_string "0x1" = 1L);
          test  "vwknw4vkl"  (Int64.of_string "0x2" = 2L);
          test  "vwknw4vkl"  (Int64.of_string "0xa" = 10L);
          test  "vwknw4vkl"  (Int64.of_string "0xff" = 255L);
          stdout.WriteLine "constants (octal)";
          test  "vwknw4vkl"  (Int64.of_string "0o0" = 0L);
          test  "vwknw4vkl"  (Int64.of_string "0o1" = 1L);
          test  "vwknw4vkl"  (Int64.of_string "0o2" = 2L);
          test  "vwknw4vkl"  (Int64.of_string "0o7" = 7L);
          test  "vwknw4vkl"  (Int64.of_string "0o10" = 8L);
          test  "vwknw4vkl"  (Int64.of_string "0o777" = 7L*64L + 7L*8L + 7L);
          test  "vwknw4vkl"  (Int64.of_string "0o111" = 64L + 8L + 1L);
          stdout.WriteLine "constants (binary)";
          test  "vwknw4vkl"  (Int64.of_string "0b0" = 0L);
          test  "vwknw4vkl"  (Int64.of_string "0b1" = 1L);
          test  "vwknw4vkl"  (Int64.of_string "0b10" = 2L);
          test  "vwknw4vkl"  (Int64.of_string "0b11" = 3L);
          test  "vwknw4vkl"  (Int64.of_string "0b00000000" = 0L);
          test  "vwknw4vkl"  (Int64.of_string "0b11111111" = 0xFFL);
          test  "vwknw4vkl"  (Int64.of_string "0b1111111100000000" = 0xFF00L);
          test  "vwknw4vkl"  (Int64.of_string "0b11111111000000001111111100000000" = 0xFF00FF00L);
          test  "vwknw4vkl"  (Int64.of_string "0b11111111111111111111111111111111" = 0xFFFFFFFFL);
          test  "vwknw4vkl"  (Int64.of_string "0b1111111100000000111111110000000011111111000000001111111100000000" = 0xFF00FF00FF00FF00L);
          test  "vwknw4vkl"  (Int64.of_string "0b1111111111111111111111111111111111111111111111111111111111111111" = 0xFFFFFFFFFFFFFFFFL);

          stdout.WriteLine "of_string: min_int";
          test  "vwknw4vkl"  (Int64.of_string "-0x8000000000000000" = Int64.min_int);
          test  "vwknw4vkl"  (Int64.of_string "-9223372036854775808" = Int64.min_int);
          test  "vwknw4vkl"  (-9223372036854775808L = Int64.min_int);
          stdout.WriteLine "done";

          stdout.WriteLine "constants (hex, UInt64)";
          test  "vwknw4vkl"  (UInt64.of_string "0x0" = 0UL);
          test  "vwknw4vkl"  (UInt64.of_string "0x1" = 1UL);
          test  "vwknw4vkl"  (UInt64.of_string "0x2" = 2UL);
          test  "vwknw4vkl"  (UInt64.of_string "0xa" = 10UL);
          test  "vwknw4vkl"  (UInt64.of_string "0xff" = 255UL);
          test  "vwknw4vkl"  (UInt64.of_string "0xffffffffffffffff" = UInt64.max_int);
          
          stdout.WriteLine "constants (octal, UInt64)";
          test  "vwknw4vkl"  (UInt64.of_string "0o0" = 0UL);
          test  "vwknw4vkl"  (UInt64.of_string "0o1" = 1UL);
          test  "vwknw4vkl"  (UInt64.of_string "0o2" = 2UL);
          test  "vwknw4vkl"  (UInt64.of_string "0o7" = 7UL);
          test  "vwknw4vkl"  (UInt64.of_string "0o10" = 8UL);
          test  "vwknw4vkl"  (UInt64.of_string "0o777" = 7UL*64UL + 7UL*8UL + 7UL);
          test  "vwknw4vkl"  (UInt64.of_string "0o111" = 64UL + 8UL + 1UL);
          
          stdout.WriteLine "constants (binary, UInt64)";
          test  "vwknw4vkl"  (UInt64.of_string "0b0" = 0UL);
          test  "vwknw4vkl"  (UInt64.of_string "0b1" = 1UL);
          test  "vwknw4vkl"  (UInt64.of_string "0b10" = 2UL);
          test  "vwknw4vkl"  (UInt64.of_string "0b11" = 3UL);
          test  "vwknw4vkl"  (UInt64.of_string "0b00000000" = 0UL);
          test  "vwknw4vkl"  (UInt64.of_string "0b11111111" = 0xFFUL);
          test  "vwknw4vkl"  (UInt64.of_string "0b1111111100000000" = 0xFF00UL);
          test  "vwknw4vkl"  (UInt64.of_string "0b11111111000000001111111100000000" = 0xFF00FF00UL);
          test  "vwknw4vkl"  (UInt64.of_string "0b11111111111111111111111111111111" = 0xFFFFFFFFUL);
          test  "vwknw4vkl"  (UInt64.of_string "0b1111111100000000111111110000000011111111000000001111111100000000" = 0xFF00FF00FF00FF00UL);
          test  "vwknw4vkl"  (UInt64.of_string "0b1111111111111111111111111111111111111111111111111111111111111111" = 0xFFFFFFFFFFFFFFFFUL);

          stdout.WriteLine "done";
          ()    


[<TestFixture>]
type public ListCompatTests() =
  
    [<Test>]
    member this.BasicTests() = 
        test "List.tryfind_indexi" (List.tryFindIndex ((=) 4) [1;2;3;4;4;3;2;1] = Some 3)




[<TestFixture>]
type public PervasivesTests() =

    [<Test>]
    member this.ExceptionMappings() = 
        check "exception mappings"  true (try int_of_string "A" |> ignore; false with Failure _ -> true | _ -> false)
        check "exception mappings"  true (try float_of_string "A" |> ignore; false with Failure _ -> true | _ -> false)

    [<Test>]
    member this.BasicTests() = 
        test "tefwiu32" (try raise Not_found with Not_found -> true | _ -> false)

        test "tefw38vj" (try raise Out_of_memory with Out_of_memory -> true | _ -> false)

        test "tefw93mvj" (try raise Division_by_zero with Division_by_zero -> true | _ -> false)

        test "tefwfewevj" (try raise Stack_overflow with Stack_overflow -> true | _ -> false)

        test "tefw9ifmevj" (try raise End_of_file with End_of_file -> true | _ -> false)

        test "atefwiu32" (try raise Not_found with Out_of_memory | Division_by_zero | Stack_overflow | End_of_file -> false | Not_found -> true | _ -> false)

        test "btefwiu32" (try raise Out_of_memory with Not_found | Division_by_zero | Stack_overflow | End_of_file -> false | Out_of_memory -> true | _ -> false)

        test "ctefwiu32" (try raise Division_by_zero with Not_found | Out_of_memory | Stack_overflow | End_of_file -> false | Division_by_zero -> true | _ -> false)

        test "dtefwiu32" (try raise Stack_overflow with Not_found | Out_of_memory | Division_by_zero | End_of_file -> false | Stack_overflow -> true | _ -> false)

        test "etefwiu32" (try raise End_of_file with Not_found | Out_of_memory | Division_by_zero |  Stack_overflow -> false | End_of_file -> true | _ -> false)

        test "ftefwiu32" (try raise Foo with Not_found | Out_of_memory | Division_by_zero |  Stack_overflow -> false | Foo -> true | _ -> false)


  
    [<Test>]
    member this.IO_EndOfLine_Translations() = 

        let checkFileContentsUsingVariousTechniques(filename) =
            using (open_in_bin filename) (fun is -> 
                let buf = Array.create 5 0uy in
                check "cewjk1" (input is buf 0 5) 5;
                check "cewjk2" buf [|104uy; 101uy; 108uy; 108uy; 111uy|];
                check "cewjk3" (input is buf 0 2) 2;
                check "cewjk4" buf [|13uy; 10uy; 108uy; 108uy; 111uy|]);

            using (open_in_bin filename) (fun is2 -> 

                check "cewjk5" (is2.Peek()) 104;
                check "cewjk6" (is2.Read()) 104;
                check "cewjk7" (is2.Read()) 101;
                check "cewjk8" (is2.Read()) 108;
                check "cewjk9" (is2.Read()) 108;
                check "cewjk0" (is2.Read()) 111;
                check "cewjkq" (is2.Read()) 13;
                check "cewjkw" (is2.Read()) 10;
                check "cewjke" (is2.Read()) (-1));

            using (open_in_bin filename) (fun is3 -> 

                check "cewjkr" (input_char is3) 'h';
                check "cewjkt" (input_char is3) 'e';
                check "cewjky" (input_char is3) 'l';
                check "cewjku" (input_char is3) 'l';
                check "cewjki" (input_char is3) 'o';
                check "cewjko" (input_char is3) '\r';
                check "cewjkp" (input_char is3) '\n';
                check "cewjka" (try input_char is3 |> ignore; false with End_of_file -> true) true);

            using (open_in_bin filename) (fun is4 -> 

                let buf4 = Array.create 5 '0' in
                check "cewjks" (input_chars is4 buf4 0 5) 5;
                check "cewjkd" (buf4) [|'h'; 'e'; 'l'; 'l'; 'o'; |];
                check "cewjkf" (input_chars is4 buf4 0 2) 2;
                check "cewjkd" (buf4) [|'\r'; '\n'; 'l'; 'l'; 'o'; |];
                check "cewjkh" (input_chars is4 buf4 0 2) 0);
            
            using (open_in filename) (fun is5 -> 

                let buf5 = Array.create 5 0uy in
                check "veswhek1" (input is5 buf5 0 5) 5;
                check "veswhek2" buf5 [|104uy; 101uy; 108uy; 108uy; 111uy|];
                check "veswhek3" (input is5 buf5 0 2)  2;
                check "veswhek4" buf5 [|13uy; 10uy; 108uy; 108uy; 111uy|];
                check "veswhek5" (input is5 buf5 0 2) 0);
            

            using (open_in filename) (fun is2 -> 

                check "veswhek6" (is2.Peek()) 104;
                check "veswhek7" (is2.Read()) 104;
                check "veswhek8" (is2.Read()) 101;
                check "veswhek9" (is2.Read()) 108;
                check "veswhek0" (is2.Read()) 108;
                check "veswhekq" (is2.Read()) 111;
                check "veswhekw" (is2.Read()) 13;
                check "veswheke" (is2.Read()) 10;
                check "veswhekr" (is2.Read()) (-1));


            using (open_in filename) (fun is3 -> 

                check "veswhekt" (input_char is3) 'h';
                check "veswheky" (input_char is3) 'e';
                check "veswheku" (input_char is3) 'l';
                check "veswheko" (input_char is3) 'l';
                check "veswhekp" (input_char is3) 'o';
                check "veswheka" (input_char is3) '\r';
                check "veswheks" (input_char is3) '\n';
                check "veswhekd" (try input_char is3 |> ignore; false with End_of_file -> true) true)

        using (open_out_bin "test.txt") (fun os -> fprintf os "hello\r\n")
        checkFileContentsUsingVariousTechniques("test.txt")
        using (open_out "test.txt") (fun os -> fprintf os "hello\r\n")
        checkFileContentsUsingVariousTechniques("test.txt")
        using (open_out "test.txt") (fun os -> os.Write (let s = "hello\r\n" in Array.init s.Length (fun i -> s.[i]) ))
        checkFileContentsUsingVariousTechniques("test.txt")
        using (open_out_bin "test.txt") (fun os -> os.Write (let s = "hello\r\n" in Array.init s.Length (fun i -> s.[i]) ))
        checkFileContentsUsingVariousTechniques("test.txt")
        using (open_out "test.txt") (fun os -> os.Write "hello\r\n")
        checkFileContentsUsingVariousTechniques("test.txt")
        using (open_out_bin "test.txt") (fun os -> os.Write "hello\r\n")
        checkFileContentsUsingVariousTechniques("test.txt")

#if FX_NO_BINARY_SERIALIZATION
#else
    [<Test>]
    member this.BinarySerialization() = 

        (* Andrez:
           It appears to me that writing the empty list into a binary channel does not work.
        *)   
          
          let file = open_out_bin "test.txt" in
          output_value file ([]: int list);
          close_out file;
          let file = open_in_bin "test.txt" in
          if (input_value file : int list) <> [] then (reportFailure "wnwve0ljkvwe");
          close_in file;
#endif



[<TestFixture>]
type public Filename_Tests() =
  
    [<Test>]
    member this.BasicTests() = 
        check "Filename.dirname1"  "C:" (Filename.dirname "C:")
        check "Filename.dirname2"  "C:\\" (Filename.dirname "C:\\")
        check "Filename.dirname3"  "c:\\" (Filename.dirname "c:\\")
        check "Filename.dirname2"  "C:/" (Filename.dirname "C:/")
        check "Filename.dirname3"  "c:/" (Filename.dirname "c:/")
        check "Filename.dirname4"  "." (Filename.dirname "")
        check "Filename.dirname5"  "\\" (Filename.dirname "\\")
        check "Filename.dirname6"  "." (Filename.dirname "a")
        // F# and OCaml do return different results for this one.
        // F# preserves the double slashes.  That seems fair enough 
        // do check "Filename.dirname2"  "\\" (Filename.dirname "\\\\")

        check "is_relative1"  false (Filename.is_relative "C:")
        check "is_relative2"  false (Filename.is_relative "C:\\")
        check "is_relative3"  false (Filename.is_relative "c:\\")
        check "is_relative4"  false (Filename.is_relative "C:/")
        check "is_relative5"  false (Filename.is_relative "c:/")
        check "is_relative6"  true (Filename.is_relative "")
        check "is_relative7"  true (Filename.is_relative ".")
        check "is_relative8"  true (Filename.is_relative "a")
        check "is_relative9"  false (Filename.is_relative "\\")
        check "is_relative10"  false (Filename.is_relative "\\\\")

        check "is_relative8"  true (Filename.is_implicit "a")
        check "is_relative8"  false (Filename.is_implicit ".\\a")
        check "is_relative8"  false (Filename.is_implicit "..\\a")

        let has_extension (s:string) = 
          (String.length s >= 1 && String.get s (String.length s - 1) = '.') 
          || System.IO.Path.HasExtension(s)

        check "has_extension 1"  false (has_extension "C:")
        check "has_extension 2"  false (has_extension "C:\\")
        check "has_extension 3"  false (has_extension "c:\\")
        check "has_extension 4"  false (has_extension "")
        check "has_extension 5"  true (has_extension ".")
        check "has_extension 6"  false (has_extension "a")
        check "has_extension 7"  true (has_extension "a.b")
        check "has_extension 8"  true (has_extension ".b")
        check "has_extension 9"  true (has_extension "c:\\a.b")
        check "has_extension 10"  true (has_extension "c:\\a.")


        check "chop_extension1"  true (try ignore(Filename.chop_extension "C:"); false with Invalid_argument  "chop_extension" -> true)
        check "chop_extension2"  true (try ignore(Filename.chop_extension "C:\\"); false with Invalid_argument  "chop_extension" -> true)
        check "chop_extension3"  true (try ignore(Filename.chop_extension "c:\\"); false with Invalid_argument  "chop_extension" -> true)
        check "chop_extension4"  true (try ignore(Filename.chop_extension "C:/"); false with Invalid_argument  "chop_extension" -> true)
        check "chop_extension5"  true (try ignore(Filename.chop_extension "c:/"); false with Invalid_argument  "chop_extension" -> true)
        check "chop_extension6"  true (try ignore(Filename.chop_extension ""); false with Invalid_argument  "chop_extension" -> true)
        check "chop_extension7"  true (try ignore(Filename.chop_extension "a"); false with Invalid_argument  "chop_extension" -> true)
        check "chop_extension8"  true (try ignore(Filename.chop_extension "c:\\a"); false with Invalid_argument  "chop_extension" -> true)
        check "chop_extension9"  true (try ignore(Filename.chop_extension "c:\\foo.b\\a"); false with Invalid_argument  "chop_extension" -> true)
        check "chop_extension10"  "" (Filename.chop_extension ".")
        check "chop_extension11"  "a" (Filename.chop_extension "a.")
        check "chop_extension12"  "c:\\a" (Filename.chop_extension "c:\\a.")

        check "Filename.dirname1"  "" (Filename.basename "C:")
        check "Filename.dirname2"  "" (Filename.basename "C:\\")
        check "Filename.dirname2"  "" (Filename.basename "c:\\")
        check "Filename.dirname2"  "" (Filename.basename "")
        check "Filename.dirname2"  "c" (Filename.basename "\\\\c")
        check "Filename.dirname2"  "" (Filename.basename "\\\\")

#if FX_NO_DOUBLE_BIT_CONVERTER
#else
[<TestFixture>]
type public Float_Tests() =
  
    [<Test>]
    member this.BasicTests() = 
        check "FloatParse.1" (Float.to_bits (Float.of_string "0.0")) 0L
        check "FloatParse.0" (Float.to_bits (Float.of_string "-0.0"))      0x8000000000000000L // (-9223372036854775808L)
        check "FloatParse.2" (Float.to_bits (Float.of_string "-1E-127"))   0xa591544581b7dec2L // (-6516334528322609470L)
        check "FloatParse.3" (Float.to_bits (Float.of_string "-1E-323"))   0x8000000000000002L // (-9223372036854775806L)
        check "FloatParse.4" (Float.to_bits (Float.of_string "-1E-324"))   0x8000000000000000L // (-9223372036854775808L)
        check "FloatParse.5" (Float.to_bits (Float.of_string "-1E-325"))   0x8000000000000000L // (-9223372036854775808L)
        check "FloatParse.6" (Float.to_bits (Float.of_string "1E-325")) 0L
        check "FloatParse.7" (Float.to_bits (Float.of_string "1E-322")) 20L
        check "FloatParse.8" (Float.to_bits (Float.of_string "1E-323")) 2L
        check "FloatParse.9" (Float.to_bits (Float.of_string "1E-324")) 0L
        check "FloatParse.A" (Float.to_bits (Float.of_string "Infinity"))  0x7ff0000000000000L // 9218868437227405312L
        check "FloatParse.B" (Float.to_bits (Float.of_string "-Infinity")) 0xfff0000000000000L // (-4503599627370496L)
        check "FloatParse.C" (Float.to_bits (Float.of_string "NaN"))       0xfff8000000000000L  // (-2251799813685248L)
        check "FloatParse.D" (Float.to_bits (Float.of_string "-NaN"))    ( // http://en.wikipedia.org/wiki/NaN
                                                                  let bit64 = System.IntPtr.Size = 8 in
                                                                  if bit64 && System.Environment.Version.Major < 4 then
                                                                      // 64-bit (on NetFx2.0) seems to have same repr for -nan and nan
                                                                      0xfff8000000000000L // (-2251799813685248L)
                                                                  else
                                                                      // 64-bit (on NetFx4.0) and 32-bit (any NetFx) seems to flip the sign bit on negation.
                                                                      // However:
                                                                      // it seems nan has the negative-bit set from the start,
                                                                      // and -nan then has the negative-bit cleared!
                                                                      0x7ff8000000000000L // 9221120237041090560L
                                                                )
#endif

#if FX_NO_COMMAND_LINE_ARGS
#else
[<TestFixture>]
type public Arg_Tests() =
  
    [<Test>]
    member this.BasicTests() = 

      let res = System.Text.StringBuilder()
      let add (x:string) = res.Append("<"^x^">") |> ignore
      Microsoft.FSharp.Compatibility.OCaml.Arg.parse_argv (ref 0) [|"main.exe";"otherA";"";"otherB"|] [] add "fred"
      check "Bug3803" (res.ToString()) "<otherA><><otherB>"
#endif  

[<TestFixture>]
type public SysTests() =
  
    [<Test>]
    member this.TestFileExists() = 

        test "dwe098" (not (Sys.file_exists "never-create-me"))

    [<Test>]
    member this.Test_Sys_remove() = 
          let os = open_out "remove-me.txt" in
          close_out os;
          test "dwe098" (Sys.file_exists "remove-me.txt" && (Sys.remove "remove-me.txt"; not (Sys.file_exists "remove-me.txt")))

    [<Test>]
    member this.Test_Sys_rename() = 
          let os = open_out "rename-me.txt" in
          close_out os;
          test "dwe098dw" (Sys.file_exists "rename-me.txt" && (Sys.rename "rename-me.txt" "remove-me.txt"; Sys.file_exists "remove-me.txt" && not (Sys.file_exists "rename-me.txt") && (Sys.remove "remove-me.txt"; not (Sys.file_exists "remove-me.txt"))))

#if FX_NO_ENVIRONMENT
#else
    [<Test>]
    member this.Test_Sys_getenv() = 
          ignore (Sys.getenv "PATH");
          test "w99ocwkm" (try ignore (Sys.getenv "VERY UNLIKELY VARIABLE"); false; with Not_found -> true)
#endif

    [<Test>]
    member this.Test_Sys_getcwd() = 
            
          let p1 = Sys.getcwd() in 
          Sys.chdir "..";
          let p2 = Sys.getcwd() in 
          test "eiojk" (p1 <> p2);
          Sys.chdir p1;
          let p3 = Sys.getcwd() in 
          test "eiojk" (p1 = p3)

#if FX_NO_PROCESS_START
#else
    [<Test>]
    member this.Test_Sys_command() = 

          test "ekj" (Sys.command "help.exe" |> ignore; true)
#endif

    [<Test>]
    member this.Test_Sys_word_size() = 

          test "ekdwq8uj" (Sys.word_size = 32 || Sys.word_size = 64)

#if FX_NO_PROCESS_DIAGNOSTICS
#else
    [<Test>]
    member this.Test_Sys_time() = 

          let t1 = ref (Sys.time()) in 
          for i = 1 to 30 do 
            let t2 = Sys.time() in 
            test "fe921lk30" (!t1 <= t2);
            t1 := t2
          done
#endif

[<TestFixture>]
type public FuncConvertTests() =
  
    [<Test>]
    member this.BasicTests() = 

        check "dwe098ce1" ((Microsoft.FSharp.Core.FuncConvert.FuncFromTupled(fun (a,b) -> a + b)) 3 4) 7
        check "dwe098ce2" ((Microsoft.FSharp.Core.FuncConvert.FuncFromTupled(fun (a,b,c) -> a + b + c)) 3 4 5) 12
        check "dwe098ce3" ((Microsoft.FSharp.Core.FuncConvert.FuncFromTupled(fun (a,b,c,d) -> a + b + c + d)) 3 4 5 5) 17
        check "dwe098ce4" ((Microsoft.FSharp.Core.FuncConvert.FuncFromTupled(fun (a,b,c,d,e) -> a + b + c + d + e)) 3 4 5 5 5) 22

        check "dwe098ce1" ((Microsoft.FSharp.Core.FuncConvert.ToFSharpFunc(System.Converter(fun a -> a + 1))) 3) 4
        check "dwe098ce1" ((Microsoft.FSharp.Core.FuncConvert.ToFSharpFunc(System.Action<_>(fun a -> ()))) 3) ()

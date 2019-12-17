#!/bin/bash
#/Users/erikkvanli/Repos/RAIC2019/aicup2019-macos
rm ./results/*

#dotnet arena/aicup2019.dll 127.0.0.1 31002 &
for i in {1..1000}
do
    # echo "game $i"
	./aicup2019 --batch-mode --config config2.json --save-results results/$i.json
	./analyze.py
done

#!/usr/bin/python3

import json, os

stats = [0, 0, 0]
score = 0
for filename in os.listdir('results'):
    f = open('results/' + filename, 'r+')
    data = json.loads(f.read())
    scores = data['results']
    if scores[0] > scores[1]: stats[0] += 1
    if scores[0] < scores[1]: stats[1] += 1
    if scores[0] == scores[1]: stats[2] += 1
    score = score + scores[0] - scores[1]

print('testbot: ' + str(stats[0]))
print('arena: ' + str(stats[1]))
print('draw: ' + str(stats[2]))
print('score: ' + str(score))


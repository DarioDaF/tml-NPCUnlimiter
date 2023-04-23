import json5 as json

with open('./log/outInfos.json', 'r') as fp:
    res = json.load(fp)

src = {}
with open('./patchlist/hardcodedLimits.json', 'r') as fp:
    src.update(json.load(fp))
with open('./patchlist/constUsage.json', 'r') as fp:
    src.update(json.load(fp))

for name in src.keys():
    s = src[name]
    r = res[name]

    new_r = {}
    for k, v in r.items():
        new_k = k.split(':')[0]
        if new_k in new_r:
            new_r[new_k] += v
        else:
            new_r[new_k] = v

    if s != new_r:
        print(name, s, new_r)

totalEntries = sum(sum(infos.values()) for infos in res.values())
print(f'Total number of entries patched: {totalEntries}')

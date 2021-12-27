cd /d %~dp0

hg add
hg remove -A
hg commit -m "Commit local changes"
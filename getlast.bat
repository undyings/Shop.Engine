cd /d %~dp0

hg pull
hg merge
hg commit -m "Merged remote changes"
hg update
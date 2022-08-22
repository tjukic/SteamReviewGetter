# RELEASE NOTES Steam Review Getter (SRG)

## EA v0.1

## Description

This tool will fetch all the *recent* reviews for a game by its **Steam app ID**.
It will save data in two files:
- a JSON structured, raw data file with all the relevant info for further machine processing (or human reading)
- an XLSX document; an export of the same data, partially formatted for easier human reading and manual (or script) processing in Microsoft Excel.

## How-to?

Simply run the program from the *CMD* or *Powershell* (recommended):

CMD
```srg.exe [steam app ID]```

e.g.
```srg.exe 2023810``` (Sample small game with low review count https://store.steampowered.com/app/2023810/Island_Idle_RPG)

PS
```.\srg.exe [steam app ID]```

e.g.
```.\srg.exe 2023810``` (Sample small game with low review count https://store.steampowered.com/app/2023810/Island_Idle_RPG)

If you have a working network connection and Steam API is working normally, you should see the program fetching data and reporting on progress.
At the end it will save both files in your local user Documents folder (i.e. My Documents) in the ```FPSRG``` subfolder.

## Gotchas

At the moment, the tool is somewhat limited. There are a few caveats and warnings.

* Currently the tool supports running only a single game review fetching process at a time. The next version will support a more autonomous, multithreaded processing, e.g. getting a list of Steam App IDs.
* Excel document export is not ideal at the moment, as it requires manual cell formatting for a few columns. That is on the list to improve.
* Steam API has a few bugs at the moment this document was written, which includes returning data formatting errors, which this tool fixes on the fly, but it is unknown if any more bugs will appear and the impact is unknown.
* Steam API for reviews is a bit quirky with filters. At the moment, we're hard-coding "recent" filter for the API call (https://partner.steamgames.com/doc/store/getreviews) and using maximum fetch batch size of 100 items. Filter "all" is either bugged or intentionally returning erratic results or same result batches within short time. We plan to expand this configurability in the future.
* Unlike the Excel XLSX export which has dual sheets, formated dates, etc., the JSON file comes as is, in a raw data form.
* Excel document is somewhat mess. Microsoft has a active breaking bug in their Office SDK. Free alternatives are extremely limited. Near future plan is to abandon the exact XLSX format and go for ODS or some other alternative document format easily convertible to Excel.
* This tool is only in CLI form at the moment; albeit in a self-contained Win-64 build that should work on Windows 7 and newer. There are **no dependencies** or at least there should be none.
* GUI is probably coming soon.
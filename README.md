# RollingBackupSweep
Identify old daily backup snapshops and slowly delete older snapshots. Keep *n* daily, weekly (7 day windows), and monthly (28 days) snapshots.

Uses .NET Native AOT to turn C# into tidy (~7MB) standalone native binaries (no .NET runtime required). Slip it into your backup container image!

## Requirements
Backup snapshots must contain yyyy-MM-dd in the filename (year-month-day); the utility does not look at file create date because some file systems change the file times (e.g. [S3](https://docs.aws.amazon.com/AmazonS3/latest/userguide/UsingMetadata.html) always uses upload date), which could result in data loss. If you need to rename files in bulk, try [rename](https://man7.org/linux/man-pages/man1/rename.1.html). E.g. I had files without dashes in the date string and renamed with `rename -n -v -e 's/^(.*)(\d{4})(\d{2})(\d{2})-(.*)/$1$2-$3-$4-T$5/' *.*` (-n is for dry-run). 


## How it Works
"Today" is always defined as the most recent date found on the filesystem. Again, to prevent data loss in the event that something went wrong with your snapshot mechanism.

As an example, if today is 2023-05-17 and you want to retain 7 daily snapshots, 2 weekly, and 3 monthly:

* The utility will look at the window from 05-11 to 5-17 and retain all daily snapshots.
* It will then look back at the two next 7-day windows (05-04 to 05-10 and 04-27 to 05-03) and, for each window, keep the *oldest* file found.
* It will then repeat with three 28-day windows (04-05 to 05-03, 03-07 to 04-04, 02-06 to 03-06), again, keeping the *oldest* file found. 28 because it's a multiple of 7 so will work well when weekly files become monthly files. 

Keeping the oldest file in each window means that new files that age into a window will be deleted if there's already an older one. Your backups will slowly fade away, like bad memories from band camp, while giving you the opportunity to go back to an old snapshot if something goes wrong. 

A great example use case something encrypted with a password that you forget - you can go back to old snapshots with a prior password (that you hopefully remember) and at least recover *some* of your data. 

## Usage

```
Description:
  Sweep backup snapshots with yyyy-MM-dd in the filename, retaining the specified number of daily, weekly, and
  monthly snapshots.

Usage:
  RollingBackupSweep [options]

Options:
  --path <path> (REQUIRED)                                Path to sweep of backup snapshots.
  --days <days> (REQUIRED)                                Number of daily backup snapshots to retain.
  --weeks <weeks> (REQUIRED)                              Number of weekly backup snapshots to retain.
  --months <months> (REQUIRED)                            Number of monthly backup snapshots to retain.
  --dry-run                                               Dry run; don't delete anything.
  --verbosity <Detailed|Diagnostic|Minimal|Normal|Quiet>  Output detail level. [default: Normal]
  --verbose                                               Verbose output. Equivalent to --verbosity Diagnostic.
  --version                                               Show version information
  -?, -h, --help                                          Show help and usage information
```

## Include in Docker Image

Be sure to change the version number and platform. Be sure to include `-L` to follow redirects, as github requires. You can also change the unzip location with `-C`.

```
RUN VERSION="v0.9.4" && curl -L -O https://github.com/pettijohn/RollingBackupSweep/releases/download/$VERSION/RollingBackupSweep-$VERSION-linux-arm64.tar.gz \
    && tar zxf RollingBackupSweep-$VERSION-linux-arm64.tar.gz -C /usr/bin/ \
    && rm RollingBackupSweep-$VERSION-linux-arm64.tar.gz
```
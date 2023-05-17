# RollingBackupSweep
Identify old daily backup snapshops and slowly delete older snapshots. Keep *n* daily, weekly (7 day windows), and monthly (28 days) snapshots.

## Requirements
Backup snapshots must contain yyyy-MM-dd in the filename (year-month-day); the utility does not look at file create date because some cloud hosts always set create date as the upload date, not the original file date (e.g. [S3](https://docs.aws.amazon.com/AmazonS3/latest/userguide/UsingMetadata.html)), which could result in data loss. 

"Today" is always defined as the most recent date found on the filesystem. Again, to prevent data loss in the event that something went wrong with your snapshot mechanism.

As an example, if today is 2023-05-17 and you want to retain 7 daily snapshots, 2 weekly, and 3 monthly:

* The utility will look at the window from 05-11 to 5-17 and retain all daily snapshots.
* It will then look back at the 2 next 7 day windows (05-04 to 05-10 and 04-27 to 05-03) and, for each window, keep the *oldest* file found.
* It will then repeat with 3 28-day windows, again, keeping the *oldest* file found. 28 because it's a multiple of 7 so will work well when weekly files become monthly files. 

## Usage

```
Description:
  Sweep backup snapshots with yyyy-MM-dd in the filename, retaining the specified number of daily, weekly, and monthly
  snapshots.

Usage:
  RollingBackupSweep [options]

Options:
  --path <path> (REQUIRED)      Path to sweep of backup snapshots.
  --days <days> (REQUIRED)      Number of daily backup snapshots to retain.
  --weeks <weeks> (REQUIRED)    Number of weekly backup snapshots to retain.
  --months <months> (REQUIRED)  Number of monthly backup snapshots to retain.
  --dry-run                     Dry run; don't delete anything.
  --verbose                     Verbose output.
  --version                     Show version information
  -?, -h, --help                Show help and usage information
```

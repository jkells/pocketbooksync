PocketBookSync
==============

What is it?
-----------
A command line tool for synchronizing transactions from bank accounts to PocketBook. Only the Commonwealth Bank is currently supported, I would accept pull requests for adding new banks.

Motivation
----------
There are a bunch of cloud based personal finance applications available now but banks typically don't provide API's. As a consequence to get the full experience in these applications you need to give them your internet banking credentials, something I'm not comfortable with.

I really liked the look of PocketBook [www.getpocketbook.com](https://www.getpocketbook.com) so decided to create a tool that would synchronize my transactions without needing to hand over my internet banking credentials. Hopefully one day my bank will provide an API so this tool isn't necessary.

Should you use this?
--------------------
If you're not able to read and build the code yourself possibly not. **You're trusting this application with your credentials instead of PocketBook.** PocketBook have hundreds of thousands of customers trusting them with their credentials, their buisness is is built on the trust they have with their customers.

How are my credentials secured
------------------------------
Your credentials are stored in an SQLite database under your user profile. They're encrypted at rest with the Microsoft Data Protection API, meaning they can only be decrypted by your user account.

Usage
-----
```
Usage:  [options] [command]

Options:
  -? | -h | --help  Show help information

Commands:
  add-account     Add a bank account
  configure       Configure pocket book credentials
  delete-account  Delete a bank account
  list            List bank accounts
  sync            Sync all bank accounts
  sync-account    Sync a bank account

Use " [command] --help" for more information about a command.
```

Quick Start
-----------
* Setup your PocketBook credentials, you will be prompted to enter your password.
```
pocketbooksync configure --username john@example.com
```
* Add an account to PocketBook and find the account number.
You can find your PocketBook account number by adding a manual bank account to PocketBook, then select Transactions followed by your account. The account number will be in the URL.
* Add your bank account to pocketbooksync, you will be prompted for your netbank password.
```
pocketbooksync add-account --client-number 123456 --account-reference MasterCard --pocket-book-account 100000 --type cba
```
* Synchronize your transactions!
```
pocketbooksync sync
```
* Create a scheduled task to run the sync command once a day.

Thanks
------
Thanks to PocketBook [www.getpocketbook.com](www.getpocketbook.com) for creating such a great budgeting app!

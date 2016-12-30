PocketBookSync
==============

What is it?
-----------
A command line tool for synchronizing transactions from bank accounts to PocketBook.

Motivation
----------
There are a bunch of cloud based personal finance applications available now but banks typically don't provide API's. As a consequence to get the full experience in these applications you need to give them your internet banking credentials, something I'm not comfortable with.

I really liked the look of PocketBook (getpocketbook.com) so decided to create a tool that would synchronize my transactions without needing to hand over my internet banking credentials.

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

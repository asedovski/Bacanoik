# Bacanoik
Paranoidal backup tool with double encryption and cloud upload possibilities.

Common idea - encrypt single file (twice due to reason below) and upload to cloud or save locally.

## Step by step

### Encryption scenario.
Execute in cmd:
```
bacanoik LOCAL E C:\Temp\test.txt password1 password2
```
Where:
* **LOCAL** - _type of target storage. (AZURE - will deploy directly to cloud but temporarly disabled)_
* **E** - _encryption direction_
* **C:\Temp\test.txt** - _file to be encrypted_
* **password1** - _password for first pass  (not required for decription, may be almost anything)_
* **password2** - _password for second pass (not required for decription, may be almost anything)_
        
After encrypting finished two files will be created:
  1) test.txt.secret - with secret information required for decryption.
  2) ~~A82BD45586CF797A176B1CBEE89A5C7C~~ - (each time new name) encrypted content. May be uploaded to any public resource
  
  Content of secret file in this example:
  ```
  <?xml version="1.0"?>
  <SecretData xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
    <DecryptedFileName>test.txt</DecryptedFileName>
    <DecryptedMD5>H3Cc0UU0P65YIrQn+GvAWQ==</DecryptedMD5>
    <IV1>xzb1M9jbHRAtc+WFh1yZog==</IV1>
    <Key1>qLw15Vz6m0uvIyg7GB4GXe0qeqyvpnM8QlOGhkSrWMs=</Key1>
    <IV2>mpO5zDOywhRwfmWlnDabkA==</IV2>
    <Key2>F17AiDXM1xduXHmlqrPboRMl57SXle4QuiDyDQutNjc=</Key2>
    <EncryptedFileName>A82BD45586CF797A176B1CBEE89A5C7C</EncryptedFileName>
    <EncryptedMD5>MWs6b9/TbJ8u5n62c/12oQ==</EncryptedMD5>
  </SecretData>
  ```
  
  **Due to those fact that we store here IV and KEY - it should be saved in protected place.**
  
### Decryption scenario
Execute in cmd:
```
bacanoik LOCAL D C:\Temp\test.txt.secret
```
  _NOTE: ~~A82BD45586CF797A176B1CBEE89A5C7C~~ file should be in the same directory with spcified secret file_
  
Where:
* **LOCAL** - _type of target storage. (AZURE - will download directly from cloud but temporarly disabled)_
* **D** - _decryption direction_
* **C:\Temp\test.txt.secret** - _secret file with all required information for decryption_
After decryption file with original name (test.txt in our example) will be placed in the same directory.    


## REMARK
Initially it was done for being able encrypt/decrypt VHD files and store them on any storage platform.

1) Double pass are used due to those fact that we have always well known structure of VHD file so have some information about decrypted file.
2) passwords are really nedeed to generate keys, but even if you will use the same passwords keys will be always different thats why we use keys in secret file but not the specified passwords.
3) Name replaced with random numbers to hide information.

So theoretically, I think - there are no information in encrypted file that may point to anything but secret file is necessary to decryption as it contain everything.

In case of hash of encrypted/decrypted file missmath decryption became inpossible (at least with my level of knowledge here)

**SO DONT USE IT AS PRIMARY BACKUP...**
   
  







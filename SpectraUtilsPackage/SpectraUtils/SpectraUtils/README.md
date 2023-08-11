﻿# Dependency
This library was created by .Net6.0

## Description

The package made to step into the world of NuGet as a jr. software developer.

## Use

```Csharp
		string name = "oĞuzHan";
        name = helper.NameEdit.NameCorrection(name);
            
        string sirName = helper.NameEdit.SirNameCorrection("karagüzel");

        string normalizedName = helper.NameEdit.StandardizeCharacters("oğuzhan");

        string userName = helper.NameEdit.CreateUserName("oğuzhan");

        string password = helper.PasswordHelper.CreatePassword(
            passwordLength:8,
            includeLowercaseLetter:true,
            includeNumber:true,
            includeUppercaseLetter:true,
            includeSpecialCharacter:true
            );

        string standartPassword = helper.PasswordHelper.CreateStardartPassword;
            
        [MinimumAge(18)]
        public DateTime birthDate { get; set; }
        [TCIDValidation] // Id number validation for Türkiye 
        public string Identity { get; set; }
        [AllowedExtensions(extensions: new string[] { ".jpg", ".jpeg", ".png", ".pdf" }, maxFileSizeInBytes:1024*1024*2)]
        public string File { get; set; }
```

## Result

Oğuzhan
KARAGÜZEL
oguzhan
oguzhan1593
&W3Juv}r (random password)
M{3jq8N3 (random password)

Validation error message!

#

You can check it on [github](https://github.com/Oguzhankaraguzel/SpectraUtils)
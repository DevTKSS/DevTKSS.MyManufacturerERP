## Issues

### Workarounds

- [Uno Platform GitHub Issue #20546](https://github.com/unoplatform/uno/issues/20546) - String TypeInfo serialization

### Uno Platform Documentation Problems

- [Uno Platform Post-Login Token Processing](https://platform.uno/docs/articles/external/uno.extensions/doc/Learn/Authentication/HowTo-WebAuthentication.html#4-process-post-login-tokens)

  While the docs are introducing us to do this for web auth, [this is not working as documented](./src/DevTKSS.MyManufacturerERP/App.xaml.cs#L116)
  Exception message:

  ```plaintext
  CS1593
  Delegate "AsyncFunc<IDictionary<string, string>, IDictionary<string, string>?>" does not accept 3 arguments.
  DevTKSS.MyManufacturerERP (net9.0-browserwasm)
  C:\Users\Sonja\source\sample apps\DevTKSS.MyManufacturerERP\DevTKSS.MyManufacturerERP\App.xaml.cs 116
  ```

  ![Screenshot of IDE Type info for parameters](./docs/images/IDE-Login-Typeinfo-screenshot.png)]

- [Uno Platform Cookie Authentication](https://platform.uno/docs/articles/external/uno.extensions/doc/Learn/Authentication/HowTo-Cookies.html) - Configure cookie options

  This is missing kind of all docs about working with cookies. The provided links are not very helpfull for learning only if you might not know the words, but still you will not know how to use that in development.

### Authentication & Server Issues

- [Setting up Ports and `launchSettings.json` for https and `CSRF`](./docs/setting-up-launchsettings.md)
- Missing Identity API endpoints in Endpoint Explorer
- Delegate signature mismatch in Uno Platform authentication [CS1593 error](#uno-platform-documentation-problems)
  -> Commented out now until some fix is known

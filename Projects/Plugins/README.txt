Style: https://msdn.microsoft.com/en-us/library/ff926074.aspx

Naming Conventions:
- Camel-casing
- Private variables are prefixed with '_' and are not instance-qualified
- Static variables are not used because they are not intrinsicly thread safe
- Method invocations are prefixed
  - 'this.' for members
  - 'ClassName.' for static
  - Note: This is to clearly indicate that methods used in plugins are stateless

Organization
- Class
  - ctor(s)
  - Interface support
  - Public properties
  - Public methods
  - Private methods
  - Private variables

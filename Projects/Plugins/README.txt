Style:

Naming Conventions:
- Camel-casing
- Private variables are prefixed with '_' and are not instance-qualified
- Static variables are not used because they are not intrinsicly thread safe
- Method invocations are prefixed
  - 'this.' for members
  - 'ClassName.' for static
  - Note: This is to clearly indicate that methods used in plugins are stateless
- Invocation of member methods are prefixed with 'this.'
- Invocation of static methods are class-qualified

Organization
- Class
  - ctor(s)
  - Public Properties
  - Public Methods
  - Private methods
  - Private variables
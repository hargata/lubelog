# Contributing Guidelines

## Asking Questions
Before submitting a question, please check the [Troubleshooting Guide](https://docs.lubelogger.com/Installation/Troubleshooting) and search through existing issues.

Ideally, the Issues tab should only consist of bug reports and feature requests instead of debugging your LubeLogger installation.

## Feature Requests
Feature Requests are cool, but we do want to avoid bloat and scope/feature creep.

LubeLogger is a Vehicle Maintenance and Fuel Mileage Tracker. 
It is not and should not be used for the following:
- Project Management Software(e.g.: Jira)
- Budget Management(e.g: YNAB/Actual)
- Inventory Management

Before submitting a feature request, please consider the following:
- Will this feature benefit more than just a niche subset of users
- Will this feature result in scope creep
- Has this feature already been requested/exist

## Bug Reports
Please fill out the issue template for bug reports.

## Security Vulnerabilities
Contact us via [Email](mailto:hargatasoftworks@gmail.com)

## Submitting Pull Requests
Pull Requests are welcome, but please consider the following:

CI/CD, Bugs, Tech-Debt Related PRs:
- Make sure changes are not breaking.
- If any dependencies are added, they must be cross-platform compatible.
- Please test changes thoroughly.

Feature Related PRs:
- Have you requested this feature in the Issues tab
- Will this feature benefit more than just a niche subset of users
- Will this feature result in scope creep
- Do these changes fix a root cause or is it simply a workaround

Ideally, you should first submit a feature request in the Issues tab before submitting a PR just in case it's something we are already working on.

Note the following:

We(the maintainers) are not responsible for cleaning up, testing, or fixing your changes.

If your changes doesn't have a wide-ranging use-case, has not been thoroughly tested, or cannot be easily maintained, we won't merge your PR.

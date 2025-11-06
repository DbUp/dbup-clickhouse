[![GitHub Workflow Status (branch)](https://img.shields.io/github/actions/workflow/status/DbUp/dbup-clickhouse/main.yml?branch=main)](https://github.com/DbUp/dbup-clickhouse/actions/workflows/main.yml?query=branch%3Amain)
[![NuGet](https://img.shields.io/nuget/dt/dbup-clickhouse.svg)](https://www.nuget.org/packages/dbup-clickhouse)
[![NuGet](https://img.shields.io/nuget/v/dbup-clickhouse.svg)](https://www.nuget.org/packages/dbup-clickhouse)
[![Prerelease](https://img.shields.io/nuget/vpre/dbup-clickhouse?color=orange&label=prerelease)](https://www.nuget.org/packages/dbup-clickhouse)

# DbUp ClickHouse support
DbUp is a .NET library that helps you to deploy changes to databases. It tracks which SQL scripts have been run already and runs the change scripts that are needed to get your database up to date. This package adds ClickHouse support.

## Getting Help
To learn more about DbUp check out the [documentation](https://dbup.readthedocs.io/en/latest/)

Please only log issue related to ClickHouse support in this repo. For cross cutting issues, please use our [main issue list](https://github.com/DbUp/DbUp/issues).

# Contributing

See the [readme in our main repo](https://github.com/DbUp/DbUp/blob/master/README.md) for how to get started and contribute.

To run the tests, start the clickhouse container by running `./start-clickhouse.ps1`
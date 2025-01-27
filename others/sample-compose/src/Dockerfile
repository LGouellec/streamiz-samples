# runtime image
FROM mcr.microsoft.com/dotnet/aspnet:7.0-alpine AS base

# install rocks db dependencies
RUN apk add librdkafka --repository=https://dl-cdn.alpinelinux.org/alpine/v3.21/community/
RUN apk add --no-cache rocksdb libstdc++
RUN ln -s /usr/lib/librocksdb.so.7 /usr/lib/librocksdb.so

# Build image
FROM mcr.microsoft.com/dotnet/sdk:7.0-alpine AS build

# install rocks db dependencies
RUN apk add librdkafka --repository=https://dl-cdn.alpinelinux.org/alpine/v3.21/community/
RUN apk add --no-cache rocksdb libstdc++
RUN ln -s /usr/lib/librocksdb.so.7 /usr/lib/librocksdb.so

WORKDIR /source

# copy sln and csproj for restore
COPY *.sln ./
COPY *.props ./
COPY */*.csproj ./

# this line depends on the csprojs having the same name as the containing folders
RUN for file in $(ls *.csproj); do mkdir -p ${file%.*} && mv $file ${file%.*}; done

# restore to cache image
RUN dotnet restore

# copy the rest
COPY . ./

# run the tests
WORKDIR /source/Streamiz.Tests
RUN dotnet test --no-restore

# publish the app
WORKDIR /source/Streamiz.Demo
RUN dotnet publish --no-restore -c Release -o ../out
RUN ls -al ../out

FROM base as final

# copy published app from build image
WORKDIR /app
COPY --from=build /source/out .

# Workaround https://github.com/confluentinc/librdkafka/issues/4897
RUN cp /usr/lib/librdkafka.so.1 /app/alpine-librdkafka.so

# that's all folks
EXPOSE 8060
ENV ASPNETCORE_URLS=http://+:8060
ENTRYPOINT [ "dotnet", "Streamiz.Demo.dll" ]